using Entidades.Comercial;
using Entidades.Maestros;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;
using Utilitarios;
using Negocio.Maestros;
using System.Globalization;
using iTextSharp.text.pdf;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using MessagingToolkit.QRCode.Codec;


namespace Sunat
{
	public class DocumentoElectronico
	{
		#region Variables Globales

		//Rutas de las carpetas
		private string RUTA_REPOSITORIO_ELECTRONICO_FACTURAS = Enumerador.RUTA_SERVIDOR_DOCUMENTO_ELECTRONICO_FACTURAS;
		private string RUTA_REPOSITORIO_ELECTRONICO_BOLETAS = Enumerador.RUTA_SERVIDOR_DOCUMENTO_ELECTRONICO_BOLETAS;
		private string RUTA_REPOSITORIO_ELECTRONICO_NOTA_CREDITO = Enumerador.RUTA_SERVIDOR_DOCUMENTO_ELECTRONICO_NOTA_CREDITO;
		private string RUTA_REPOSITORIO_ELECTRONICO_NOTA_DEBITO = Enumerador.RUTA_SERVIDOR_DOCUMENTO_ELECTRONICO_NOTA_DEBITO;
		
		private string RUTA_REPOSITORIO_ELECTRONICO_GUIA_REMISION = Enumerador.RUTA_SERVIDOR_DOCUMENTO_ELECTRONICO_GUIA_REMISION;
		private string RUTA_REPOSITORIO_ELECTRONICO_LIQUIDACION_COMPRA = Enumerador.RUTA_SERVIDOR_DOCUMENTO_ELECTRONICO_LIQUIDACION_COMPRA;


		private string RUTA_SERVIDOR_DOCUMENTO_ELECTRONICO_CERTIFICADO_RUTA = Enumerador.RUTA_SERVIDOR_DOCUMENTO_ELECTRONICO_CERTIFICADO_RUTA;

		private string CTA_BANCO_NACION_DETRACCIONES = string.Empty;
		private Empresa oEmpresa = new Empresa();
		private string strNombreArchivo = string.Empty;

		XmlDocument xdocCR = new XmlDocument();

		#endregion

		#region GenerarNombreXML

		/// <summary>
		///  Genera el nombre del archivo xml de acuerdo a las transacciones
		/// </summary>
		/// <param name="oTransaccion"></param>
		/// <param name="p_oEmpresa"></param>
		/// <returns></returns>
		public string GenerarNombreXML(Transaccion oTransaccion, Empresa p_oEmpresa)
		{
			oEmpresa = p_oEmpresa;

			string sNombreArchivoXml = string.Empty;
			try
			{
				switch (oTransaccion.TipoDocumento_Id)
				{
					#region 02 - Factura - > sunat 01

					case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_FACTURA:

						sNombreArchivoXml = @"\" + oEmpresa.sEmpRuc + "-01-" + oTransaccion.sNumeroGenerado;

						break;

					#endregion

					#region 01 - Boleta - > sunat 03

					case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_BOLETA:

						sNombreArchivoXml = @"\" + oEmpresa.sEmpRuc + "-03-" + oTransaccion.sNumeroGenerado;

						break;

					#endregion

					#region 19 - Nota de Crédito - > sunat 07

					case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_NOTA_CREDITO:

						sNombreArchivoXml = @"\" + oEmpresa.sEmpRuc + "-07-" + oTransaccion.sNumeroGenerado;

						break;

					#endregion

					#region  Nota de Debito - > sunat 08

					case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_NOTA_DEBITO:

						sNombreArchivoXml = @"\" + oEmpresa.sEmpRuc + "-08-" + oTransaccion.sNumeroGenerado;

						break;

					#endregion

					#region  Guia de Remision - > sunat 09

					case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_GUIA_REMISION:

						sNombreArchivoXml = @"\" + oEmpresa.sEmpRuc + "-09-" + oTransaccion.sNumeroGenerado;

						break;

					#endregion

					#region  Liquidacion de  Compra - > sunat 04

					case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_LIQUIDACION_COMPRA:

						sNombreArchivoXml = @"\" + oEmpresa.sEmpRuc + "-04-" + oTransaccion.sNumeroGenerado;

						break;

						#endregion
				}

				return sNombreArchivoXml;
			}
			catch (Exception)
			{
				throw;
			}
		}
		#endregion

		#region Generar Archivo de Envío a Sunat XML y ZIP

		/// <summary>
		///  Genera un archivo xml y lo convierte en zip
		/// en la ruta especificada
		/// </summary>
		/// <param name="oTransaccion"></param>
		/// <param name="p_oEmpresa"></param>
		public bool GenerarArchivoXmlyZip(Transaccion oTransaccion, Empresa p_oEmpresa)
		{
			try
			{
				CTA_BANCO_NACION_DETRACCIONES = p_oEmpresa.sEmpCtaBancoDetracciones.Replace("-", "");
				oEmpresa = p_oEmpresa;

				#region CREAR DOCUMENTO ELECTRONICO

				#region INICIALES

				strNombreArchivo = GenerarNombreXML(oTransaccion, oEmpresa);

				string sMoneda = string.Empty;
				//Texto para mostrar en el importe pasado a letras
				if (oTransaccion.Moneda_Id == Enumerador.MONEDA_EXTRANJERA_DOLARES_ID)
					sMoneda = @"Dolares Americanos";
				else
					sMoneda = @"Soles";

				string CODIGO_DOCUMENTO_SUNAT = string.Empty;

				//texto para mostrar CURRENCYID-> MONEDA EN PEN O USD
				string MONEDA = oTransaccion.Moneda_Id == Enumerador.MONEDA_NACIONAL_SOLES_ID ? "PEN" : "USD";

				#endregion

				#region CREAR DIRECTORIO SI NO EXISTE

				CrearDirectorio(oTransaccion.TipoDocumento_Id, strNombreArchivo, oEmpresa);

				#endregion

				#region PREPARAR EL ESCRITOR DE XML

				XmlTextWriter W = null;

				#region NOTA DE CREDITO

				if (oTransaccion.TipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_NOTA_CREDITO)
				{
					W =
						new XmlTextWriter(oEmpresa.sEmpRuta +
							RUTA_REPOSITORIO_ELECTRONICO_NOTA_CREDITO + strNombreArchivo + @"\" + strNombreArchivo +
							".xml",
							Encoding.GetEncoding("ISO-8859-1"));
				}
				#endregion

				#region FACTURAS

				else
				{
					W =
						new XmlTextWriter(oEmpresa.sEmpRuta +
							RUTA_REPOSITORIO_ELECTRONICO_FACTURAS + strNombreArchivo + @"\" + strNombreArchivo +
							".xml",
							Encoding.GetEncoding("ISO-8859-1"));
				}

				#endregion

				W.Formatting = Formatting.Indented;
				W.WriteStartDocument(false);
				#endregion

				#region INICIO DE DOCUMENTO

				switch (oTransaccion.TipoDocumento_Id)
				{
					#region 02 - Factura - > sunat 01

					case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_FACTURA:
						W.WriteStartElement("Invoice"); //  Documento utilizado para requerir un pago
						W.WriteStartAttribute("xmlns");
						W.WriteValue("urn:oasis:names:specification:ubl:schema:xsd:Invoice-2");
						CODIGO_DOCUMENTO_SUNAT = "01";

						break;

					#endregion

					#region 01 - Boleta - > sunat 03

					case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_BOLETA:
						CODIGO_DOCUMENTO_SUNAT = "03";
						break;

					#endregion

					#region 19 - Nota de Crédito - > sunat 07

					case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_NOTA_CREDITO:
						W.WriteStartElement("CreditNote"); //  Documento utilizado para requerir un pago
						W.WriteStartAttribute("xmlns");
						W.WriteValue("urn:oasis:names:specification:ubl:schema:xsd:CreditNote-2");
						CODIGO_DOCUMENTO_SUNAT = "07";
						break;

					#endregion

					#region 49 - Nota de Debito - > sunat 08

					case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_NOTA_DEBITO:
						W.WriteStartElement("DebitNote"); //  Documento utilizado para requerir un pago
						W.WriteStartAttribute("xmlns");
						W.WriteValue("urn:oasis:names:specification:ubl:schema:xsd:DebitNote-2");
						CODIGO_DOCUMENTO_SUNAT = "08";
						break;

						#endregion
				}

				#endregion

				#region NECESARIOS SEGUN MODELO SUNAT XML

				W.WriteStartAttribute("xmlns:cac");
				W.WriteValue("urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
				W.WriteStartAttribute("xmlns:cbc");
				W.WriteValue("urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
				W.WriteStartAttribute("xmlns:ds");
				W.WriteValue("http://www.w3.org/2000/09/xmldsig#");
				W.WriteStartAttribute("xmlns:ext");
				W.WriteValue("urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2");
				W.WriteEndAttribute();

				#endregion

				//CUERPO PRINCIPAL DE CONTENIDO
				/*===============================================*/
				W.WriteStartElement("ext:UBLExtensions");
				/*===============================================*/

				//EXTENSION PARA INCRUSTAR LA FIRMA DIGITAL*/
				//==================================================================
				/*  Contenedor de Componentes de extensión. 
                    Podrán incorporarse nuevas definiciones estructuradas 
                    cuando sean de interés conjunto para emisores y receptores, 
                    y no estén ya definidas en el esquema de la factura. */

				W.WriteStartElement("ext:UBLExtension");
				W.WriteStartElement("ext:ExtensionContent");
				W.WriteEndElement();
				W.WriteEndElement();//ext:UBLExtension

				W.WriteEndElement(); //</ext:UBLExtension>

				/*--FIN DE EXTENSIONES UBL*/
				W.WriteStartElement("cbc:UBLVersionID"); //Versión del UBL
				W.WriteValue("2.1");
				W.WriteEndElement();
				W.WriteStartElement("cbc:CustomizationID"); //Versión de la estructura del documento
				W.WriteValue("2.0");
				W.WriteEndElement();
				W.WriteStartElement("cbc:ID");
				W.WriteValue(oTransaccion.sNumeroGenerado);
				W.WriteEndElement();
				W.WriteStartElement("cbc:IssueDate");
				W.WriteValue(oTransaccion.dTraFecha.ToString("yyyy-MM-dd"));
				W.WriteEndElement();

				#region FACTURA O BOLETA

				if (
					oTransaccion.TipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_FACTURA ||
					oTransaccion.TipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_BOLETA
					)
				{
					W.WriteStartElement("cbc:InvoiceTypeCode");
					W.WriteStartAttribute("listID");

					if (oTransaccion.nTraTieneDetraccion == Enumerador.ESTADO_ACTIVO)
						W.WriteValue("1001");
					else 
					if (oTransaccion.nCliImportacion == Enumerador.ESTADO_ACTIVO)
						W.WriteValue("0200");

					else
						W.WriteValue("0101");

					W.WriteEndAttribute();
					W.WriteValue(CODIGO_DOCUMENTO_SUNAT);
					W.WriteEndElement(); //cbc:PayableAmount

					W.WriteStartElement("cbc:Note");
					W.WriteStartAttribute("languageLocaleID");
					W.WriteValue("1000");
					W.WriteEndAttribute();
					W.WriteValue(Util.ConvertirNumerosAletras(oTransaccion.nTraImporteTotal, sMoneda));
					W.WriteEndElement(); //cbc:Note

					if (oTransaccion.nTraTieneDetraccion == Enumerador.ESTADO_ACTIVO)
					{
						W.WriteStartElement("cbc:Note");
						W.WriteStartAttribute("languageLocaleID");
						W.WriteValue("2006");
						W.WriteEndAttribute();
						W.WriteValue("Operación sujeta a detracción");
						W.WriteEndElement(); //cbc:Note
					}

					if (oTransaccion.nTraEsGratuito == Enumerador.ESTADO_ACTIVO)
					{
						W.WriteStartElement("cbc:Note");
						W.WriteStartAttribute("languageLocaleID");
						W.WriteValue("1002");
						W.WriteEndAttribute();
						W.WriteValue("TRANSFERENCIA GRATUITA DE UN BIEN Y/O SERVICIO PRESTADO GRATUITAMENTE");
						W.WriteEndElement(); //cbc:Note
					}
				}
				else if (
							oTransaccion.TipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_NOTA_CREDITO ||
							oTransaccion.TipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_NOTA_DEBITO
						)
				{
					W.WriteStartElement("cbc:Note");
					W.WriteStartAttribute("languageLocaleID");
					W.WriteValue("1000");
					W.WriteEndAttribute();
					W.WriteValue(Util.ConvertirNumerosAletras(oTransaccion.nTraImporteTotal, sMoneda));
					W.WriteEndElement(); //cbc:Note

				}

				#endregion

				W.WriteStartElement("cbc:DocumentCurrencyCode");
				W.WriteValue(MONEDA);
				W.WriteEndElement();

				#region NOTA DE CREDITO O NOTA DE DEBITO

				if (
					oTransaccion.TipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_NOTA_CREDITO ||
					oTransaccion.TipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_NOTA_DEBITO
					)
				{
					// ***** Motivo por el cual se emite la nota de Credito
					W.WriteStartElement("cac:DiscrepancyResponse");
					W.WriteStartElement("cbc:ReferenceID"); //Identifica el numero que se aplica la nota de Credito
					W.WriteValue(oTransaccion.sSerieNumeroRelacionado);
					W.WriteEndElement();

					#region NOTA DE CREDITO O NOTA DE DEBITO

					//Código por el cual se emite la nota de credito
					//tambien se toma en cuenta el codigo como nota de debito osea por documento
					W.WriteStartElement("cbc:ResponseCode");
					W.WriteValue(oTransaccion.nCReCodigoSunat);
					W.WriteEndElement();
					W.WriteStartElement("cbc:Description"); //Descripcion Nota Credito
					W.WriteValue(oTransaccion.sCategoriaReclamo);
					W.WriteEndElement();

					#endregion

					W.WriteEndElement(); //DiscrepancyResponse

					W.WriteStartElement("cac:BillingReference"); //Referencia a un documento modificado

					W.WriteStartElement("cac:InvoiceDocumentReference"); //Asociacion al documento modificado

					W.WriteStartElement("cbc:ID"); //Documento que se modifica (serie-numero)
					W.WriteValue(oTransaccion.sSerieNumeroRelacionado);
					W.WriteEndElement();

					#region SI EL DOCUMENTO RELACIONADO DE LA NOTA DE CREDITO O DEBITO ES FACTURA

					W.WriteStartElement("cbc:DocumentTypeCode"); //Codigo del Tipo de Documento modificado

					if (oTransaccion.nTipoDocRelacionado == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_FACTURA)
					{
						W.WriteValue("01");
					}

					else
					{
						W.WriteValue("03");
					}

					W.WriteEndElement();//DocumentTypeCode

					#endregion

					W.WriteEndElement();//InvoiceDocumentReference

					W.WriteEndElement();//BillingReference

				}

				#endregion

				#region REFERENCIA A LA FIRMA DIGITAL

				/*Nota: Solo anteponer la letra "S" 
                * luego el numero completo serienumero del correlativo de la transaccion*/

				W.WriteStartElement("cac:Signature"); //Referencia a la Firma Digital
				W.WriteStartElement("cbc:ID"); //Identificador de la firma
				W.WriteValue("S" + oTransaccion.sNumeroGenerado);
				W.WriteEndElement();

				#endregion

				// ***** Datos Empresa certificado ******
				W.WriteStartElement("cac:SignatoryParty"); //Código del tipo de documento adicional (p.e. SCOP)

				W.WriteStartElement("cac:PartyIdentification"); //Parte firmante

				W.WriteStartElement("cbc:ID"); //Identificación de la parte firmante
											   //AGREGAR EL RUC DE LA EMPRESA EMISORA
				W.WriteValue(oEmpresa.sEmpRuc);
				W.WriteEndElement();//cbc:ID

				W.WriteEndElement();//PartyIdentification

				W.WriteStartElement("cac:PartyName");
				W.WriteStartElement("cbc:Name"); //Nombre de la parte firmante
				W.WriteValue(oEmpresa.sEmpNombre);
				W.WriteEndElement();//Name
				W.WriteEndElement();//PartyName

				W.WriteEndElement();//SignatoryParty

				W.WriteStartElement("cac:DigitalSignatureAttachment");
				W.WriteStartElement("cac:ExternalReference");
				W.WriteStartElement("cbc:URI");
				//Identificador de Recurso Uniforme (o URI) que identifica la localización de la firma
				/*Nota: Solo anteponer el caracter "#" 
                * luego el numero completo serienumero del correlativo de la transaccion*/
				W.WriteValue("#" + oTransaccion.sNumeroGenerado);
				W.WriteEndElement();
				W.WriteEndElement();
				W.WriteEndElement();
				W.WriteEndElement(); //cac:Signature
									 //nuevamente
									 // ***** Empresa *****
				W.WriteStartElement("cac:AccountingSupplierParty"); //Datos del Emisor del documento

				/*Nota: 
                 * RECEPTOR CLIENTE SE PONEN LO MISMO.
                 Por lo tanto se sigue el mismo standar*/

				#region NOMBRE DE LA EMPRESA EMISORA

				W.WriteStartElement("cac:Party");

				W.WriteStartElement("cac:PartyIdentification");
				W.WriteStartElement("cbc:ID");
				W.WriteStartAttribute("schemeID");
				W.WriteValue(oEmpresa.sEmpTipoIdentificadorEmpresaSunat);
				W.WriteEndAttribute();
				W.WriteValue(oEmpresa.sEmpRuc);
				W.WriteEndElement();//cbc:ID
				W.WriteEndElement();//cac:PartyIdentification

				W.WriteStartElement("cac:PartyName");
				W.WriteStartElement("cbc:Name"); //Nombre Comercial
				W.WriteValue(oEmpresa.sEmpNombre);
				W.WriteEndElement();//cbc:Name
				W.WriteEndElement();//PartyName

				W.WriteStartElement("cac:PartyLegalEntity");

				W.WriteStartElement("cbc:RegistrationName");
				W.WriteValue(oEmpresa.sEmpNombre);
				W.WriteEndElement();//cbc:RegistrationName

				W.WriteStartElement("cac:RegistrationAddress");

				W.WriteStartElement("cbc:ID");
				W.WriteValue(oEmpresa.sEmpCodigoUbigeoSunat);
				W.WriteEndElement();//cbc:ID


				W.WriteStartElement("cbc:AddressTypeCode"); // Departamento

				W.WriteValue("0000");//nuevo 13-02-2021

				W.WriteEndElement();//cbc:AddressTypeCode


				W.WriteStartElement("cbc:CityName"); // Departamento
				W.WriteValue(oEmpresa.sEmpCiudad);
				W.WriteEndElement();//cbc:CityName
				W.WriteStartElement("cac:AddressLine"); // Departamento
				W.WriteStartElement("cbc:Line"); // Departamento
				W.WriteValue(oEmpresa.sEmpDireccion);
				W.WriteEndElement();//cbc:Line
				W.WriteEndElement();//cac:AddressLine

				W.WriteEndElement();//cac:RegistrationAddress
									//--

				W.WriteEndElement();//cac:PartyLegalEntity

				#endregion

				W.WriteEndElement(); //cac:Party         

				W.WriteEndElement(); //cac:AccountingSupplierParty

				W.WriteStartElement("cac:AccountingCustomerParty");
				W.WriteStartElement("cac:Party");

				#region BOLETA

				if (oTransaccion.TipoDocumento_Id ==
					(int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_BOLETA)
				{
					W.WriteStartElement("cac:PartyIdentification");
					W.WriteStartElement("cbc:ID");
					W.WriteStartAttribute("schemeID");

					if (oTransaccion.sCliTipoIdentidad.Length <= 0)
					{
						//SEGUN LOS CODIGOS DE SUNAT 1-> DNI Y 6->RUC
						if (oTransaccion.sCliNroIdentidad.Length == 8)
							W.WriteValue("1");
						else //SI ES RUC:NO SE DA PERO PODRI SER NECESARIO
							if (oTransaccion.sCliNroIdentidad.Length == 11)
							//SEGUN LOS CODIGOS DE SUNAT 1-> DNI Y 6->RUC
							W.WriteValue("6");
					}
					else
					{  //soporte para personas del extranjero
						W.WriteValue(oTransaccion.sCliTipoIdentidad);
					}

					W.WriteEndAttribute();
					W.WriteValue(oTransaccion.sCliNroIdentidad);
					W.WriteEndElement();
					W.WriteEndElement();
				}
				#endregion


				#region DIFERENTE DE BOLETA : FACTURA, ETC MOSTRAMOS RUC

				else
				{
					W.WriteStartElement("cac:PartyIdentification");
					W.WriteStartElement("cbc:ID"); //Nombre Comercial
					W.WriteStartAttribute("schemeID");

					if (oTransaccion.sCliTipoIdentidad.Length <= 0)
					{
						//SEGUN LOS CODIGOS DE SUNAT 1-> DNI Y 6->RUC
						if (oTransaccion.sCliNroIdentidad.Length == 8)
							W.WriteValue("1");
						else //SI ES RUC:NO SE DA PERO PODRI SER NECESARIO
							if (oTransaccion.sCliNroIdentidad.Length == 11)
							//SEGUN LOS CODIGOS DE SUNAT 1-> DNI Y 6->RUC
							W.WriteValue("6");
					}
					else
					{  //soporte para personas del extranjero
						W.WriteValue(oTransaccion.sCliTipoIdentidad);
					}

					W.WriteEndAttribute();
					W.WriteValue(oTransaccion.sCliNroIdentidad);
					W.WriteEndElement();
					W.WriteEndElement();
				}

				#endregion

				#region NOMBRE DEL CLIENTE Y DIRECCION

				//--------------------------------------------
				W.WriteStartElement("cac:PartyLegalEntity");

				W.WriteStartElement("cbc:RegistrationName");
				W.WriteValue(oTransaccion.sCliente);
				W.WriteEndElement();//RegistrationName

				W.WriteStartElement("cac:RegistrationAddress");

				W.WriteStartElement("cac:AddressLine");
				W.WriteStartElement("cbc:Line");
				W.WriteValue(oTransaccion.sCliDomicilioFiscal);
				W.WriteEndElement();//RegistrationName
				W.WriteEndElement();//AddressLine


				W.WriteStartElement("cac:Country");
				W.WriteStartElement("cbc:IdentificationCode");
				W.WriteValue(oTransaccion.sCliPaisBreve);
				W.WriteEndElement();//cbc:IdentificationCode
				W.WriteEndElement();//cac:Country


				W.WriteEndElement();//RegistrationAddress


				W.WriteEndElement();//PartyLegalEntity
				//---------------------------------------------



				if (oTransaccion.TipoDocumento_Id ==
					(int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_FACTURA

					||
					oTransaccion.TipoDocumento_Id ==
					(int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_NOTA_CREDITO

					)
				{

					if (oTransaccion.sCliTelefono.Length > 0 && oTransaccion.sCliEmail.Length > 0)
					{
						#region Datos del contacto del comprador
						//--------------------------------------------
						W.WriteStartElement("cac:Contact");

						W.WriteStartElement("cbc:Telephone");
						W.WriteValue(oTransaccion.sCliTelefono);
						W.WriteEndElement();//Telephone

						W.WriteStartElement("cbc:ElectronicMail");
						W.WriteValue(oTransaccion.sCliEmail);
						W.WriteEndElement();//ElectronicMail

						W.WriteEndElement();//cac:Contact
											//--------------------------------------------
						#endregion Datos del contacto del comprador
					}
				}


				W.WriteEndElement();//Party


				W.WriteEndElement(); //cac:AccountingCustomerParty

				#endregion

				#region DETRACCION

				if (oTransaccion.nTraTieneDetraccion == Enumerador.ESTADO_ACTIVO)
				{
					//---
					W.WriteStartElement("cac:PaymentMeans");

					W.WriteStartElement("cbc:ID");
					W.WriteValue("Detraccion");
					W.WriteEndElement();//cbc:ID

					W.WriteStartElement("cbc:PaymentMeansCode");
					W.WriteValue("001");
					W.WriteEndElement();//cbc:PaymentMeansCode
					W.WriteStartElement("cac:PayeeFinancialAccount");
					W.WriteStartElement("cbc:ID");
					W.WriteValue(CTA_BANCO_NACION_DETRACCIONES);
					W.WriteEndElement();//cbc:ID
					W.WriteEndElement();//cac:PayeeFinancialAccount
					W.WriteEndElement();//cac:PaymentMeans
										//--

					//--
					W.WriteStartElement("cac:PaymentTerms");

					W.WriteStartElement("cbc:ID");
					W.WriteValue("Detraccion");
					W.WriteEndElement();//cbc:ID

					W.WriteStartElement("cbc:PaymentMeansID");
					W.WriteValue(oEmpresa.sEmpCodigoDetraccion);
					W.WriteEndElement();//cbc:PaymentMeansID

					W.WriteStartElement("cbc:PaymentPercent");
					W.WriteValue(String.Format(CultureInfo.InvariantCulture, "{0:0.00}", oTransaccion.nTraPctjDetraccion));
					W.WriteEndElement();//cbc:PaymentPercent

					W.WriteStartElement("cbc:Amount");
					W.WriteStartAttribute("currencyID");
					W.WriteValue("PEN");
					W.WriteEndAttribute();
					W.WriteValue(String.Format(CultureInfo.InvariantCulture, "{0:0.00}", oTransaccion.nTraMontoDetraccion));
					W.WriteEndElement(); //</cbc:PayableAmount>

					W.WriteEndElement();//cac:PaymentTerms
										//--
				}

				#endregion


				#region FORMA DE PAGO FACTURA

				if (oTransaccion.TipoDocumento_Id ==
					 (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_FACTURA

					 //||
					 //oTransaccion.TipoDocumento_Id ==
					 //(int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_NOTA_CREDITO

					 )
				{
					W.WriteStartElement("cac:PaymentTerms");
					W.WriteStartElement("cbc:ID");
					W.WriteValue("FormaPago");
					W.WriteEndElement();//cbc:ID

					W.WriteStartElement("cbc:PaymentMeansID");

					if (oTransaccion.nTraFormaPago == 2)
						W.WriteValue("Credito");
					else
						W.WriteValue("Contado");

					W.WriteEndElement();//cbc:PaymentMeansID

					if (oTransaccion.nTraFormaPago == 2)
					{
						W.WriteStartElement("cbc:Amount");
						W.WriteStartAttribute("currencyID");
						W.WriteValue(MONEDA);
						W.WriteEndAttribute();
						W.WriteValue(String.Format(CultureInfo.InvariantCulture, "{0:0.00}", oTransaccion.nTraImporteTotal));
						W.WriteEndElement();//cbc:Amount
					}
					W.WriteEndElement();//cac:PaymentTerms


					//Detalle de la forma de pago en cuotas
					if (oTransaccion.nTraFormaPago == 2)
					{
						if (oTransaccion.LstFormaPago.Count > 0)
						{

							foreach (var item in oTransaccion.LstFormaPago)
							{

								W.WriteStartElement("cac:PaymentTerms");
								W.WriteStartElement("cbc:ID");
								W.WriteValue("FormaPago");
								W.WriteEndElement();//cbc:ID

								W.WriteStartElement("cbc:PaymentMeansID");
								//cuotas nombre de cuota
								W.WriteValue(item.sFPaNombreCuota);
								//
								W.WriteEndElement();//cbc:PaymentMeansID

								W.WriteStartElement("cbc:Amount");
								W.WriteStartAttribute("currencyID");
								W.WriteValue(MONEDA);
								W.WriteEndAttribute();
								W.WriteValue(String.Format(CultureInfo.InvariantCulture, "{0:0.00}", item.nFPaTotalCuota));
								W.WriteEndElement();//cbc:Amount

								W.WriteStartElement("cbc:PaymentDueDate");
								W.WriteValue(item.dFPaFechaCuota.ToString("yyyy-MM-dd"));
								W.WriteEndElement();//cbc:PaymentDueDate

								W.WriteEndElement();//cac:PaymentTerms
							}
						}
					}
				}

				#endregion


				#region FORMA DE PAGO NOTAS

				if (oTransaccion.TipoDocumento_Id ==
					 (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_NOTA_CREDITO
					 )
				{

					if (oTransaccion.nTraFormaPago == 2)
					{
						W.WriteStartElement("cac:PaymentTerms");
						W.WriteStartElement("cbc:ID");
						W.WriteValue("FormaPago");
						W.WriteEndElement();//cbc:ID

						W.WriteStartElement("cbc:PaymentMeansID");

						//if (oTransaccion.nTraFormaPago == 2)
						W.WriteValue("Credito");
						//else
						//    W.WriteValue("Contado");

						W.WriteEndElement();//cbc:PaymentMeansID

						//if (oTransaccion.nTraFormaPago == 2)
						//{
						W.WriteStartElement("cbc:Amount");
						W.WriteStartAttribute("currencyID");
						W.WriteValue(MONEDA);
						W.WriteEndAttribute();
						W.WriteValue(String.Format(CultureInfo.InvariantCulture, "{0:0.00}", oTransaccion.nTraImporteTotal));
						W.WriteEndElement();//cbc:Amount
											// }
						W.WriteEndElement();//cac:PaymentTerms

						//Detalle de la forma de pago en cuotas
						//if (oTransaccion.nTraFormaPago == 2)
						//{
						if (oTransaccion.LstFormaPago.Count > 0)
						{
							foreach (var item in oTransaccion.LstFormaPago)
							{
								W.WriteStartElement("cac:PaymentTerms");
								W.WriteStartElement("cbc:ID");
								W.WriteValue("FormaPago");
								W.WriteEndElement();//cbc:ID

								W.WriteStartElement("cbc:PaymentMeansID");
								//cuotas nombre de cuota
								W.WriteValue(item.sFPaNombreCuota);
								//
								W.WriteEndElement();//cbc:PaymentMeansID

								W.WriteStartElement("cbc:Amount");
								W.WriteStartAttribute("currencyID");
								W.WriteValue(MONEDA);
								W.WriteEndAttribute();
								W.WriteValue(String.Format(CultureInfo.InvariantCulture, "{0:0.00}", item.nFPaTotalCuota));
								W.WriteEndElement();//cbc:Amount

								W.WriteStartElement("cbc:PaymentDueDate");
								W.WriteValue(item.dFPaFechaCuota.ToString("yyyy-MM-dd"));
								W.WriteEndElement();//cbc:PaymentDueDate

								W.WriteEndElement();//cac:PaymentTerms
							}
						}
						//}
					}
				}

				#endregion


				// ***** IGV
				W.WriteStartElement("cac:TaxTotal"); //Impuestos Globales

				W.WriteStartElement("cbc:TaxAmount"); //Importe total de un tributo para la factura
				W.WriteStartAttribute("currencyID");
				W.WriteValue(MONEDA);
				W.WriteEndAttribute();

				if (oTransaccion.nTraEsGratuito == Enumerador.ESTADO_ACTIVO)
				{
					W.WriteValue(String.Format(CultureInfo.InvariantCulture, "{0:0.00}", oTransaccion.nTraMontoIGV));
				}
				else
				{
					W.WriteValue(String.Format(CultureInfo.InvariantCulture, "{0:0.00}", oTransaccion.nTraMontoIGV));
				}


				W.WriteEndElement(); //cbc:TaxAmount
				W.WriteStartElement("cac:TaxSubtotal");

				W.WriteStartElement("cbc:TaxableAmount"); //Importe total de un tributo para la factura <cbc:TaxableAmount currencyID="PEN">200.00</cbc:TaxableAmount>
				W.WriteStartAttribute("currencyID");
				W.WriteValue(MONEDA);
				W.WriteEndAttribute();
				W.WriteValue(String.Format(CultureInfo.InvariantCulture, "{0:0.00}", oTransaccion.nTraSubTotalSinIGV));
				W.WriteEndElement(); //cbc:TaxableAmount

				W.WriteStartElement("cbc:TaxAmount"); //Importe total de un tributo para la factura
				W.WriteStartAttribute("currencyID"); //Importe explícito a tributar
				W.WriteValue(MONEDA);
				W.WriteEndAttribute();

				if (oTransaccion.nTraEsGratuito == Enumerador.ESTADO_ACTIVO)
				{
					W.WriteValue(String.Format(CultureInfo.InvariantCulture, "{0:0.00}", oTransaccion.nTraMontoIGV));
				}
				else
				{
					W.WriteValue(String.Format(CultureInfo.InvariantCulture, "{0:0.00}", oTransaccion.nTraMontoIGV));
				}

				W.WriteEndElement(); //cbc:TaxAmount  

				W.WriteStartElement("cac:TaxCategory");
				W.WriteStartElement("cac:TaxScheme");
				W.WriteStartElement("cbc:ID"); //Identificación del tributo según Catálogo No. 05


				if (oEmpresa.nEmpExonerado == Enumerador.ESTADO_ACTIVO)
				{
					W.WriteValue("9997");
				}
				else
				{
					if (oTransaccion.nTraEsGratuito == Enumerador.ESTADO_ACTIVO)
					{
						W.WriteValue("9996");
					}
					else 
					if (oTransaccion.nCliImportacion == Enumerador.ESTADO_ACTIVO)
					{
						W.WriteValue("9995");
					}

					else
					if (oTransaccion.LstTransaccionDetalle[0].nTDeEstadoIGV == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_IGV_EXONERADO)
					{
						W.WriteValue("9997");
					}
					else
						if (oTransaccion.LstTransaccionDetalle[0].nTDeEstadoIGV
					   == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_IGV_NO_GRABADO)
					{
						W.WriteValue("9998");
					}
					else
					{
						W.WriteValue("1000");
					}
				}

				W.WriteEndElement();
				W.WriteStartElement("cbc:Name"); //Nombre del Tributo (IGV, ISC)


				if (oTransaccion.nTraEsGratuito == Enumerador.ESTADO_ACTIVO)
				{
					W.WriteValue("GRA");
				}
				else
				if (oTransaccion.nCliImportacion == Enumerador.ESTADO_ACTIVO)
				{
					W.WriteValue("EXP");
				}
				else

				/*SI LA EMPRSA ES NAFECTA AL IGV*/
				/*---------------------------------------------------*/
				if (oEmpresa.nEmpExonerado == Enumerador.ESTADO_ACTIVO)
				{
					W.WriteValue("EXO");
					W.WriteEndElement();
					W.WriteStartElement("cbc:TaxTypeCode"); //Código del Tipo de Tributo (UN/ECE 5153)
					W.WriteValue("VAT");
				}
				else
				{
					if (oTransaccion.LstTransaccionDetalle[0].nTDeEstadoIGV == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_IGV_EXONERADO)
					{
						W.WriteValue("EXO");
					}
					else
						if (oTransaccion.LstTransaccionDetalle[0].nTDeEstadoIGV
					   == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_IGV_NO_GRABADO)
					{
						W.WriteValue("INA");
					}
					else
					{
						W.WriteValue("IGV");
					}


					W.WriteEndElement();
					W.WriteStartElement("cbc:TaxTypeCode"); //Código del Tipo de Tributo (UN/ECE 5153)


					if (oTransaccion.nCliImportacion == Enumerador.ESTADO_ACTIVO)
					{
						W.WriteValue("FRE");
					}
					else
					if (oTransaccion.nTraEsGratuito == Enumerador.ESTADO_ACTIVO)
					{
						W.WriteValue("FRE");
					}
					else
					if (oTransaccion.LstTransaccionDetalle[0].nTDeEstadoIGV
					   == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_IGV_EXONERADO)
					{
						W.WriteValue("VAT");
					}
					else
						if (oTransaccion.LstTransaccionDetalle[0].nTDeEstadoIGV
					   == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_IGV_NO_GRABADO)
					{
						W.WriteValue("FRE");
					}
					else
					{
						W.WriteValue("VAT");
					}

				}

				W.WriteEndElement();//cbc:TaxTypeCode
				W.WriteEndElement();
				W.WriteEndElement(); //cac:TaxCategory

				//--
				W.WriteEndElement(); //cac:TaxSubtotal

				#region ICBPER
				if (oTransaccion.nTraICBPER == Enumerador.ESTADO_ACTIVO)
				{
					/*COMPLEMENTARIO: SI ES QUE TIENE OTROS IMPUESTOS
					 LOGICA: SI EL CAMPO ICBPER=1 ENTONCES SE ESCRIBIRÁ
					 EN EL XML EL VALOR CORRESPONDIENTE
					 */
					W.WriteStartElement("cac:TaxSubtotal");
					//--------------------------------------------
					W.WriteStartElement("cbc:TaxAmount");
					W.WriteStartAttribute("currencyID"); //Importe explícito a tributar MONEDA
					W.WriteValue(MONEDA);
					W.WriteEndAttribute();

					//este monto debe de cuadrar con la sumatoria de ICBPER del detalle
					W.WriteValue(String.Format(CultureInfo.InvariantCulture, "{0:0.00}", oTransaccion.nTraMontoICBPER));
					W.WriteEndElement(); //cbc:TaxAmount  
										 //--------------------------------------------
					W.WriteStartElement("cac:TaxCategory");
					W.WriteStartElement("cac:TaxScheme");
					W.WriteStartElement("cbc:ID"); //Identificación del tributo según Catálogo No. 05
					W.WriteValue("7152");//MODIFICACION DE CODIGO CON RESPECTO AL IMPUESTO A LAS BOLSAS
					W.WriteEndElement();
					W.WriteStartElement("cbc:Name");
					W.WriteValue("ICBPER");
					W.WriteEndElement();
					W.WriteStartElement("cbc:TaxTypeCode");
					W.WriteValue("OTH");
					W.WriteEndElement();//cbc:TaxTypeCode
					W.WriteEndElement();//cac:TaxScheme
					W.WriteEndElement(); //cac:TaxCategory
										 //--
					W.WriteEndElement(); //cac:TaxSubtotal ICBPER
										 /*--------------------------------------------------*/
				}

				#endregion

				W.WriteEndElement(); //cac:TaxTotal


				if (oTransaccion.TipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_NOTA_DEBITO)
				{
					W.WriteStartElement("cac:RequestedMonetaryTotal"); //Totales a pagar de la Factura y Cargos
				}
				else
					W.WriteStartElement("cac:LegalMonetaryTotal"); //Totales a pagar de la Factura y Cargos
		
				if (
					 oTransaccion.TipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_FACTURA ||
					 oTransaccion.TipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_BOLETA
					 )
				{
					W.WriteStartElement("cbc:LineExtensionAmount"); //<cbc:LineExtensionAmount>
					W.WriteStartAttribute("currencyID");
					W.WriteValue(MONEDA);
					W.WriteEndAttribute();

					if (oTransaccion.nTraEsGratuito == Enumerador.ESTADO_ACTIVO)
						W.WriteValue("0.00");
					else
						W.WriteValue(String.Format(CultureInfo.InvariantCulture,
							"{0:0.00}", (oTransaccion.LstTransaccionDetalle.Sum(x=> x.nTDeBase))));

					W.WriteEndElement(); //cbc:LineExtensionAmount

					//--
					W.WriteStartElement("cbc:TaxInclusiveAmount"); //<cbc:LineExtensionAmount>
					W.WriteStartAttribute("currencyID");
					W.WriteValue(MONEDA);
					W.WriteEndAttribute();
					W.WriteValue(String.Format(CultureInfo.InvariantCulture, "{0:0.00}", oTransaccion.nTraImporteTotal));
					W.WriteEndElement(); //cbc:LineExtensionAmount

				}

				W.WriteStartElement("cbc:PayableAmount");
				W.WriteStartAttribute("currencyID");
				W.WriteValue(MONEDA);
				W.WriteEndAttribute();

				if (oTransaccion.nTraEsGratuito == Enumerador.ESTADO_ACTIVO)
					W.WriteValue("0.00");
				else
					W.WriteValue(String.Format(CultureInfo.InvariantCulture, "{0:0.00}", oTransaccion.nTraImporteTotal));

				W.WriteEndElement(); //cbc:PayableAmount
				W.WriteEndElement(); //cac:LegalMonetaryTotal

				GenerarDetalleXml(oTransaccion, W, oEmpresa);

				// ***** Copiando el XML *****
				W.WriteEndElement(); // Cerrando Nodos Principales
									 // ***** Grabando datos del XML *****
				W.Flush();
				W.Close();

				#endregion

				#region  INCRUSTAR CERTIFICADO

				Certificado(strNombreArchivo, oTransaccion.TipoDocumento_Id, p_oEmpresa);

				#endregion

				#region COMPRIMIR EL ARCHIVO XML FORMADO A ZIP

				ComprimirXML(strNombreArchivo, oTransaccion.TipoDocumento_Id, p_oEmpresa);

				#endregion


				return true;
			}
			catch (Exception ex)
			{
				LogApplicationNeg.Instance.GuardarLogAplicacion(ex);
				return false;
			}
		}

		public bool GenerarArchivoGuiaRemisionXmlyZip(Transaccion oTransaccion, Empresa p_oEmpresa)
		{
			try
			{
				CTA_BANCO_NACION_DETRACCIONES = p_oEmpresa.sEmpCtaBancoDetracciones.Replace("-", "");
				oEmpresa = p_oEmpresa;

				#region CREAR DOCUMENTO ELECTRONICO

				#region INICIALES

				strNombreArchivo = GenerarNombreXML(oTransaccion, oEmpresa);

				string sMoneda = string.Empty;
				//Texto para mostrar en el importe pasado a letras
				if (oTransaccion.Moneda_Id == Enumerador.MONEDA_EXTRANJERA_DOLARES_ID)
					sMoneda = @"Dolares Americanos";
				else
					sMoneda = @"Soles";

				string CODIGO_DOCUMENTO_SUNAT = string.Empty;

				//texto para mostrar CURRENCYID-> MONEDA EN PEN O USD
				string MONEDA = oTransaccion.Moneda_Id == Enumerador.MONEDA_NACIONAL_SOLES_ID ? "PEN" : "USD";

				#endregion

				#region CREAR DIRECTORIO SI NO EXISTE

				CrearDirectorio(oTransaccion.TipoDocumento_Id, strNombreArchivo, oEmpresa);

				#endregion

				#region PREPARAR EL ESCRITOR DE XML

				XmlTextWriter W = null;

				#region GUIA DE REMISION

				W = new XmlTextWriter(oEmpresa.sEmpRuta +
							RUTA_REPOSITORIO_ELECTRONICO_GUIA_REMISION + strNombreArchivo + @"\" + strNombreArchivo +
							".xml",
							Encoding.GetEncoding("ISO-8859-1"));

				#endregion

				W.Formatting = Formatting.Indented;
				W.WriteStartDocument(false);
				#endregion

				#region INICIO DE DOCUMENTO


				W.WriteStartElement("DespatchAdvice"); //  Documento utilizado para requerir un pago
				W.WriteStartAttribute("xmlns");
				W.WriteValue("urn:oasis:names:specification:ubl:schema:xsd:DespatchAdvice-2");
				CODIGO_DOCUMENTO_SUNAT = "09";

				#endregion

				#region NECESARIOS SEGUN MODELO SUNAT XML

				W.WriteStartAttribute("xmlns:cac");
				W.WriteValue("urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");

				W.WriteStartAttribute("xmlns:cbc");
				W.WriteValue("urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");

				W.WriteStartAttribute("xmlns:ccts");
				W.WriteValue("urn:un:unece:uncefact:documentation:2");

				W.WriteStartAttribute("xmlns:ds");
				W.WriteValue("http://www.w3.org/2000/09/xmldsig#");

				W.WriteStartAttribute("xmlns:ext");
				W.WriteValue("urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2");

				W.WriteStartAttribute("xmlns:qdt");
				W.WriteValue("urn:oasis:names:specification:ubl:schema:xsd:QualifiedDatatypes-2");

				W.WriteStartAttribute("xmlns:sac");
				W.WriteValue("urn:sunat:names:specification:ubl:peru:schema:xsd:SunatAggregateComponents-1");

				W.WriteStartAttribute("xmlns:udt");
				W.WriteValue("urn:un:unece:uncefact:data:specification:UnqualifiedDataTypesSchemaModule:2");

				W.WriteStartAttribute("xmlns:xsi");
				W.WriteValue("http://www.w3.org/2001/XMLSchema-instance");


				W.WriteEndAttribute();

				#endregion

				//CUERPO PRINCIPAL DE CONTENIDO
				/*===============================================*/
				W.WriteStartElement("ext:UBLExtensions");
				/*===============================================*/

				//EXTENSION PARA INCRUSTAR LA FIRMA DIGITAL*/
				//==================================================================

				W.WriteStartElement("ext:UBLExtension");
				W.WriteStartElement("ext:ExtensionContent");
				W.WriteEndElement();
				W.WriteEndElement();//ext:UBLExtension

				W.WriteEndElement(); //</ext:UBLExtension>

				/*--FIN DE EXTENSIONES UBL*/
				W.WriteStartElement("cbc:UBLVersionID"); //Versión del UBL
				W.WriteValue("2.1");
				W.WriteEndElement();
				W.WriteStartElement("cbc:CustomizationID"); //Versión de la estructura del documento
				W.WriteValue("1.0");
				W.WriteEndElement();
				W.WriteStartElement("cbc:ID");
				W.WriteValue(oTransaccion.sNumeroGenerado);
				W.WriteEndElement();
				W.WriteStartElement("cbc:IssueDate");
				W.WriteValue(oTransaccion.dTraFecha.ToString("yyyy-MM-dd"));
				W.WriteEndElement();

				W.WriteStartElement("cbc:DespatchAdviceTypeCode");
				W.WriteValue(CODIGO_DOCUMENTO_SUNAT);
				W.WriteEndElement();

				W.WriteStartElement("cbc:Note");
				W.WriteValue("Guia de Remision");
				W.WriteEndElement();

				#region REFERENCIA A LA FIRMA DIGITAL

				/*Nota: Solo anteponer la letra "S" 
                * luego el numero completo serienumero del correlativo de la transaccion*/

				W.WriteStartElement("cac:Signature"); //Referencia a la Firma Digital
				W.WriteStartElement("cbc:ID"); //Identificador de la firma
				W.WriteValue("S" + oTransaccion.sNumeroGenerado);
				W.WriteEndElement();

				#endregion

				#region SignatoryParty
				// ***** Datos Empresa certificado ******
				W.WriteStartElement("cac:SignatoryParty"); //Código del tipo de documento adicional (p.e. SCOP)

				W.WriteStartElement("cac:PartyIdentification"); //Parte firmante

				W.WriteStartElement("cbc:ID"); //Identificación de la parte firmante
											   //AGREGAR EL RUC DE LA EMPRESA EMISORA
				W.WriteValue(oEmpresa.sEmpRuc);
				W.WriteEndElement();//cbc:ID

				W.WriteEndElement();//PartyIdentification

				W.WriteStartElement("cac:PartyName");
				W.WriteStartElement("cbc:Name"); //Nombre de la parte firmante
				W.WriteValue(oEmpresa.sEmpNombre);
				W.WriteEndElement();//Name
				W.WriteEndElement();//PartyName

				W.WriteEndElement();//SignatoryParty
				#endregion

				//----------------------------------------------------------
				W.WriteStartElement("cac:DigitalSignatureAttachment");
				W.WriteStartElement("cac:ExternalReference");
				W.WriteStartElement("cbc:URI");
				//Identificador de Recurso Uniforme (o URI) que identifica la localización de la firma
				/*Nota: Solo anteponer el caracter "#" 
                * luego el numero completo serienumero del correlativo de la transaccion*/
				W.WriteValue("#" + oTransaccion.sNumeroGenerado);
				W.WriteEndElement();
				W.WriteEndElement();//ExternalReference
				W.WriteEndElement();//DigitalSignatureAttachment
				W.WriteEndElement(); //cac:Signature

				//nuevamente
				// ***** Empresa *****
				#region DespatchSupplierParty

				W.WriteStartElement("cac:DespatchSupplierParty"); //Datos del Emisor del documento

				W.WriteStartElement("cbc:CustomerAssignedAccountID");
				W.WriteStartAttribute("schemeID");
				W.WriteValue(oEmpresa.sEmpTipoIdentificadorEmpresaSunat);
				W.WriteEndAttribute();
				W.WriteValue(oEmpresa.sEmpRuc);
				W.WriteEndElement();//cbc:CustomerAssignedAccountID

				/*Nota: 
                 * RECEPTOR CLIENTE SE PONEN LO MISMO.
                 Por lo tanto se sigue el mismo standar*/
				#region Party
				W.WriteStartElement("cac:Party");
				W.WriteStartElement("cac:PartyLegalEntity");
				W.WriteStartElement("cbc:RegistrationName");
				W.WriteValue(oEmpresa.sEmpNombre);
				W.WriteEndElement();//cbc:RegistrationName
				W.WriteEndElement();//cac:PartyLegalEntity
				W.WriteEndElement();//Party
				#endregion

				W.WriteEndElement();//DespatchSupplierParty

				#endregion

				#region DeliveryCustomerParty
				//CustomerAssignedAccountID 
				W.WriteStartElement("cac:DeliveryCustomerParty"); //Datos del Emisor del documento

				W.WriteStartElement("cbc:CustomerAssignedAccountID");
				W.WriteStartAttribute("schemeID");

				if (oTransaccion.sCliNroIdentidad.Length == 8)
					W.WriteValue("1");//schemeID
				else //SI ES RUC:NO SE DA PERO PODRI SER NECESARIO
					if (oTransaccion.sCliNroIdentidad.Length == 11)
					//SEGUN LOS CODIGOS DE SUNAT 1-> DNI Y 6->RUC
					W.WriteValue("6");//schemeID
									  /*RUC DEL CLIENTE*/
				W.WriteEndAttribute();
				W.WriteValue(oTransaccion.sCliNroIdentidad);
				W.WriteEndElement();//cbc:CustomerAssignedAccountID

				#region Party
				W.WriteStartElement("cac:Party");
				W.WriteStartElement("cac:PartyLegalEntity");
				W.WriteStartElement("cbc:RegistrationName");
				W.WriteValue(oTransaccion.sCliente);
				W.WriteEndElement();//cbc:RegistrationName
				W.WriteEndElement();//cac:PartyLegalEntity
				W.WriteEndElement();//Party
				#endregion

				W.WriteEndElement();//DeliveryCustomerParty

				#endregion

				#region Shipment

				W.WriteStartElement("cac:Shipment");
				W.WriteStartElement("cbc:ID");
				W.WriteValue("1");
				W.WriteEndElement();


				//Codigo Motivo del traslado  -Catálogo N° 20
				W.WriteStartElement("cbc:HandlingCode");
				W.WriteValue(oTransaccion.sTraCodigoMotivoTraslado);
				W.WriteEndElement();//HandlingCode

				W.WriteStartElement("cbc:Information");
				W.WriteValue(oTransaccion.sTraMotivoTraslado);
				W.WriteEndElement();//Information

				W.WriteStartElement("cbc:GrossWeightMeasure");
				W.WriteStartAttribute("unitCode");
				//Unidad de medida del peso bruto
				W.WriteValue(oTransaccion.sTraCodigoUnidadMedidaTraslado);
				W.WriteEndAttribute();

				//Peso bruto total de los guía
				//W.WriteValue(oTransaccion.nTraPesoBrutoGuiaTraslado);
				W.WriteValue(String.Format(CultureInfo.InvariantCulture, "{0:0.000}", oTransaccion.nTraPesoBrutoGuiaTraslado));
				//W.WriteValue(50);

				W.WriteEndElement();//cbc:GrossWeightMeasure



				//Indicador de Transbordo Programado
				W.WriteStartElement("cbc:SplitConsignmentIndicator");
				W.WriteValue(false);
				W.WriteEndElement();//SplitConsignmentIndicator




				//--
				W.WriteStartElement("cac:ShipmentStage");
				//Modalidad de Traslado Catalogo Nro 18 01:Publico; 02:Privado
				W.WriteStartElement("cbc:TransportModeCode");
				W.WriteValue(oTransaccion.sTraCodigoModalidadTraslado);
				W.WriteEndElement();//TransportModeCode

				W.WriteStartElement("cac:TransitPeriod");
				W.WriteStartElement("cbc:StartDate");
				W.WriteValue(oTransaccion.dTraFechaTraslado.ToString("yyyy-MM-dd"));
				W.WriteEndElement();//cac:StartDate
				W.WriteEndElement();//</cac:TransitPeriod>

				W.WriteStartElement("cac:CarrierParty");

				W.WriteStartElement("cac:PartyIdentification");
				W.WriteStartElement("cbc:ID");
				W.WriteValue(oTransaccion.sTraRUCTransportistaTraslado);
				W.WriteEndElement();//ID
				W.WriteEndElement();//PartyIdentification

				W.WriteStartElement("cac:PartyName");
				W.WriteStartElement("cbc:Name");
				W.WriteValue(oTransaccion.sTraNombreTransportistaTraslado);
				W.WriteEndElement();//Name
				W.WriteEndElement();//PartyName

				W.WriteEndElement();//</cac:CarrierParty>

				//--
				W.WriteStartElement("cac:TransportMeans");

				W.WriteStartElement("cac:RoadTransport");

				//Numero de placa del vehiculo
				W.WriteStartElement("cbc:LicensePlateID");
				W.WriteValue(oTransaccion.sTraPlacaVehiculoTraslado);
				W.WriteEndElement();//LicensePlateID
				W.WriteEndElement();//RoadTransport

				W.WriteEndElement();//</cac:TransportMeans>

				W.WriteStartElement("cac:DriverPerson");
				W.WriteStartElement("cbc:ID ");
				W.WriteStartAttribute("schemeID");
				W.WriteValue("1");
				W.WriteEndAttribute();
				W.WriteValue(oTransaccion.sTraDNIConductorTraslado);
				W.WriteEndElement();//ID
				W.WriteEndElement();//DriverPerson

				W.WriteEndElement();//ShipmentStage

				#region Delivery

				W.WriteStartElement("cac:Delivery");
				//Direccion del Punto de Llegada
				W.WriteStartElement("cac:DeliveryAddress");

				//Ubigeo del origen
				W.WriteStartElement("cbc:ID");
				W.WriteValue(oTransaccion.sTraCodigoUbigeoPuntoLlegadaTraslado);
				W.WriteEndElement();//ID

				//Direccion completa y derallada
				W.WriteStartElement("cbc:StreetName");
				W.WriteValue(oTransaccion.sTraDireccionPuntoLlegadaTraslado);
				W.WriteEndElement();//StreetName

				W.WriteStartElement("cac:Country");
				W.WriteStartElement("cbc:IdentificationCode");
				W.WriteValue(oEmpresa.sEmpAbreviaturaPais);//ppr defecto dejamos PE pero podria ser oto y mejor
				W.WriteEndElement();//IdentificationCode
				W.WriteEndElement();//Country
				W.WriteEndElement();//DeliveryAddress


				W.WriteEndElement();//Delivery
				#endregion end Delivery

				#region OriginAddress


				//Direccion del punto de partida
				W.WriteStartElement("cac:OriginAddress");

				//Ubigeo del origen
				W.WriteStartElement("cbc:ID");
				W.WriteValue(oTransaccion.sTraCodigoUbigeoPuntoPartidaTraslado);
				W.WriteEndElement();//ID

				//Direccion completa y derallada
				W.WriteStartElement("cbc:StreetName");
				W.WriteValue(oTransaccion.sTraDireccionPuntoPartidaTraslado);
				W.WriteEndElement();//StreetName

				W.WriteStartElement("cac:Country");
				W.WriteStartElement("cbc:IdentificationCode");
				W.WriteValue(oEmpresa.sEmpAbreviaturaPais);//ppr defecto dejamos PE pero podria ser oto y mejor
				W.WriteEndElement();//IdentificationCode
				W.WriteEndElement();//Country
				W.WriteEndElement();//OriginAddress
				#endregion end OriginAddress

				W.WriteEndElement();//Shipment
				#endregion end Shipment


				GenerarDetalleGuiaRemisionXml(oTransaccion, W);

				// ***** Copiando el XML *****
				W.WriteEndElement(); // Cerrando Nodos Principales
									 // ***** Grabando datos del XML *****
				W.Flush();
				W.Close();

				#endregion

				#region  INCRUSTAR CERTIFICADO

				Certificado(strNombreArchivo, oTransaccion.TipoDocumento_Id, p_oEmpresa);

				#endregion

				#region COMPRIMIR EL ARCHIVO XML FORMADO A ZIP

				ComprimirXML(strNombreArchivo, oTransaccion.TipoDocumento_Id, p_oEmpresa);

				#endregion


				return true;
			}
			catch (Exception ex)
			{
				LogApplicationNeg.Instance.GuardarLogAplicacion(ex);
				return false;
			}
		}


		#region Detalle de la Guia de Remision
		private void GenerarDetalleGuiaRemisionXml(Transaccion oTransaccion, XmlTextWriter W)
		{
			try
			{
				int i = 0;
				foreach (var item in oTransaccion.LstTransaccionDetalle)
				{
					i++;

					W.WriteStartElement("cac:DespatchLine"); //Ítems de Guia

					W.WriteStartElement("cbc:ID"); // Número de orden del Ítem
					W.WriteValue(i);
					W.WriteEndElement();//ID
					W.WriteStartElement("cbc:DeliveredQuantity"); // Unidad de medida por Ítem (UN/ECE rec 20)
					W.WriteStartAttribute("unitCode");
					W.WriteValue(item.sUMeCodigoSunat);
					W.WriteEndAttribute();
					W.WriteValue(String.Format(CultureInfo.InvariantCulture, "{0:0.00}", item.nTDeCantidad));
					W.WriteEndElement();//DeliveredQuantity

					W.WriteStartElement("cac:OrderLineReference");
					W.WriteStartElement("cbc:LineID"); // Número de orden del Ítem
					W.WriteValue(i);
					W.WriteEndElement();//LineID
					W.WriteEndElement();//OrderLineReference

					W.WriteStartElement("cac:Item");

					W.WriteStartElement("cbc:Name");
					W.WriteValue(item.sProNombre);
					W.WriteEndElement();//Name

					W.WriteStartElement("cac:SellersItemIdentification");

					W.WriteStartElement("cbc:ID");
					W.WriteValue(item.Producto_Id);
					W.WriteEndElement();//ID

					W.WriteEndElement();//SellersItemIdentification

					W.WriteEndElement();//Item
										//--

					W.WriteEndElement();//DespatchLine
				}

			}
			catch (Exception)
			{

				throw;
			}
		}
		#endregion


		#endregion

		#region GenerarDetalleXml

		/// <summary>
		/// Genera el detalle del documento xml iterando cada
		/// detalle de la ransaccion venta
		/// </summary>
		/// <param name="oTransaccion"></param>
		/// <param name="W"></param>
		private void GenerarDetalleXml(Transaccion oTransaccion, XmlTextWriter W, Empresa p_oEmpresa)
		{
			try
			{
				int i = 0;
				string MONEDA = oTransaccion.Moneda_Id == Enumerador.MONEDA_NACIONAL_SOLES_ID ? "PEN" : "USD";

				foreach (var item in oTransaccion.LstTransaccionDetalle)
				{
					i++;

					#region NOTA DE CREDITO

					if (oTransaccion.TipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_NOTA_CREDITO)
					//Nota de Credito
					{
						W.WriteStartElement("cac:CreditNoteLine"); //Ítems de Nota Credito
						W.WriteStartElement("cbc:ID"); // Número de orden del Ítem
						W.WriteValue(i);
						W.WriteEndElement();
						W.WriteStartElement("cbc:CreditedQuantity"); // Unidad de Medida por Ítem (UN/ECE rec 20)
						W.WriteStartAttribute("unitCode"); //Importe explícito a tributar Moneda e Importe total a pagar
						W.WriteValue(item.sUMeCodigoSunat);
						W.WriteEndAttribute();

						W.WriteValue(String.Format(CultureInfo.InvariantCulture, "{0:0.00}", item.nTDeCantidad));
						W.WriteEndElement();
					}
					#endregion

					#region NOTA DE DEBITO

					else if (oTransaccion.TipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_NOTA_DEBITO)
					//Nota de debito
					{
						W.WriteStartElement("cac:DebitNoteLine"); //Ítems de Nota debito
						W.WriteStartElement("cbc:ID"); // Número de orden del Ítem
						W.WriteValue(i);
						W.WriteEndElement();
						W.WriteStartElement("cbc:DebitedQuantity"); // Unidad de Medida por Ítem (UN/ECE rec 20)
						W.WriteStartAttribute("unitCode");
						W.WriteValue(item.sUMeCodigoSunat);
						W.WriteEndAttribute();

						W.WriteValue(String.Format(CultureInfo.InvariantCulture, "{0:0.00}", item.nTDeCantidad));
						W.WriteEndElement();
					}

					#endregion
					else
					{
						W.WriteStartElement("cac:InvoiceLine"); //Ítems de Factura

						W.WriteStartElement("cbc:ID"); // Número de orden del Ítem
						W.WriteValue(i);
						W.WriteEndElement();
						W.WriteStartElement("cbc:InvoicedQuantity"); // Unidad de medida por Ítem (UN/ECE rec 20)
						W.WriteStartAttribute("unitCode");
						W.WriteValue(item.sUMeCodigoSunat);
						W.WriteEndAttribute();

						W.WriteValue(String.Format(CultureInfo.InvariantCulture, "{0:0.00}", item.nTDeCantidad));
						W.WriteEndElement();
					}
					///////////
					W.WriteStartElement("cbc:LineExtensionAmount");
					W.WriteStartAttribute("currencyID");
					W.WriteValue(MONEDA);
					W.WriteEndAttribute();

					if (oTransaccion.nTraEsGratuito == Enumerador.ESTADO_ACTIVO)
					{
						W.WriteValue("0.00");
					}
					else
						W.WriteValue(String.Format(CultureInfo.InvariantCulture, "{0:0.00}", item.nTDeBase));
					//W.WriteValue(String.Format(CultureInfo.InvariantCulture, "{0:0.00}", item.nTDeSubtotal));

					W.WriteEndElement();

					W.WriteStartElement("cac:PricingReference"); //Valores unitarios

					if (oTransaccion.TipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_FACTURA
						&&
						oTransaccion.nTraEsGratuito == Enumerador.ESTADO_ACTIVO
						)
					{
						W.WriteStartElement("cac:AlternativeConditionPrice"); //Valores unitarios
						W.WriteStartElement("cbc:PriceAmount"); //Monto del valor unitario
						W.WriteStartAttribute("currencyID");
						W.WriteValue(MONEDA);
						W.WriteEndAttribute();
						W.WriteValue(String.Format(CultureInfo.InvariantCulture, "{0:0.00}", item.nTDePrecio));
						W.WriteEndElement();
						W.WriteStartElement("cbc:PriceTypeCode"); //Código del valor unitario
						W.WriteValue("02");
						W.WriteEndElement();
					}
					else if (oTransaccion.TipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_BOLETA
							   &&
							   oTransaccion.nTraEsGratuito == Enumerador.ESTADO_ACTIVO
							 )
					{
						W.WriteStartElement("cac:AlternativeConditionPrice"); //Valores unitarios
						W.WriteStartElement("cbc:PriceAmount"); //Monto del valor unitario
						W.WriteStartAttribute("currencyID");
						W.WriteValue(MONEDA);
						W.WriteEndAttribute();
						W.WriteValue(String.Format(CultureInfo.InvariantCulture, "{0:0.00}", item.nTDePrecio));
						W.WriteEndElement();
						W.WriteStartElement("cbc:PriceTypeCode"); //Código del valor unitario
						W.WriteValue("02");
						W.WriteEndElement();
					}

					else
					{
						W.WriteStartElement("cac:AlternativeConditionPrice"); //Valores unitarios
						W.WriteStartElement("cbc:PriceAmount"); //Monto del valor unitario
						W.WriteStartAttribute("currencyID");
						//Importe explícito a tributar Moneda e Importe total a pagar
						W.WriteValue(MONEDA);
						W.WriteEndAttribute();
						W.WriteValue(String.Format(CultureInfo.InvariantCulture, "{0:0.00}", item.nTDePrecio));
						W.WriteEndElement();
						W.WriteStartElement("cbc:PriceTypeCode"); //Código del valor unitario
						W.WriteValue("01");
						W.WriteEndElement();//cbc:PriceTypeCode"
					}

					W.WriteEndElement();// </cac:AlternativeConditionPrice>

					W.WriteEndElement(); // </cac:PricingReference>

					#region Factura
					if (oTransaccion.nTraEsGratuito == Enumerador.ESTADO_ACTIVO
						&& oTransaccion.TipoDocumento_Id ==
						(int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_FACTURA)
					{
						// ***** Igv Total del Documento *****
						W.WriteStartElement("cac:TaxTotal");
						W.WriteStartElement("cbc:TaxAmount"); //Importe total de un tributo para este ítem
						W.WriteStartAttribute("currencyID");
						W.WriteValue(MONEDA);
						W.WriteEndAttribute();
						W.WriteValue(String.Format(CultureInfo.InvariantCulture, "{0:0.00}", item.nTDeIGV));
						W.WriteEndElement();

						// ***** Igv x Producto *****
						W.WriteStartElement("cac:TaxSubtotal");

						W.WriteStartElement("cbc:TaxableAmount");
						W.WriteStartAttribute("currencyID");
						W.WriteValue(MONEDA);
						W.WriteEndAttribute();
						//W.WriteValue(String.Format(CultureInfo.InvariantCulture, "{0:0.00}", item.nTDeBase));
						W.WriteValue(String.Format(CultureInfo.InvariantCulture, "{0:0.00}", item.nTDeSubtotal));
						W.WriteEndElement();

						W.WriteStartElement("cbc:TaxAmount");
						W.WriteStartAttribute("currencyID");
						W.WriteValue(MONEDA);
						W.WriteEndAttribute();
						W.WriteValue(String.Format(CultureInfo.InvariantCulture, "{0:0.00}", item.nTDeIGV));
						W.WriteEndElement();

						W.WriteStartElement("cac:TaxCategory"); //Afectación del IGV (Catálogo No. 07)

						W.WriteStartElement("cbc:Percent"); //Porcentaje de Sunat aplicado
						W.WriteValue("18");//por defecto
						W.WriteEndElement();
						W.WriteStartElement("cbc:TaxExemptionReasonCode");

						if (oEmpresa.nEmpExonerado == Enumerador.ESTADO_ACTIVO)
						{
							W.WriteValue("20");
						}
						else
						{
							W.WriteValue("11");
						}


						W.WriteEndElement();

						W.WriteStartElement("cbc:TaxScheme");

						W.WriteStartElement("cbc:ID"); //Identificación del tributo según Catálogo No. 05

						if (oEmpresa.nEmpExonerado == Enumerador.ESTADO_ACTIVO)
						{
							W.WriteValue("9997");
							W.WriteEndElement();

							W.WriteStartElement("cbc:Name"); //Identificación del tributo según Catálogo No. 05
							W.WriteValue("EXO");
							W.WriteEndElement();

							W.WriteStartElement("cbc:TaxTypeCode");
							W.WriteValue("VAT");
							W.WriteEndElement();
						}
						else
						{

							W.WriteValue("9996");
							W.WriteEndElement();

							W.WriteStartElement("cbc:Name"); //Identificación del tributo según Catálogo No. 05
							W.WriteValue("GRA");
							W.WriteEndElement();

							W.WriteStartElement("cbc:TaxTypeCode");
							W.WriteValue("FRE");
							W.WriteEndElement();
						}

						W.WriteEndElement();//TaxScheme
					}
					#endregion
					else

					#region  Boleta

						if (oTransaccion.nTraEsGratuito == Enumerador.ESTADO_ACTIVO
							&& oTransaccion.TipoDocumento_Id ==
							(int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_BOLETA)
					{
						// ***** Igv Total del Documento *****
						W.WriteStartElement("cac:TaxTotal");
						W.WriteStartElement("cbc:TaxAmount"); //Importe total de un tributo para este ítem
						W.WriteStartAttribute("currencyID");
						W.WriteValue(MONEDA);
						W.WriteEndAttribute();
						W.WriteValue(String.Format(CultureInfo.InvariantCulture, "{0:0.00}", item.nTDeIGV));
						W.WriteEndElement();

						// ***** Igv x Producto *****
						W.WriteStartElement("cac:TaxSubtotal");

						W.WriteStartElement("cbc:TaxableAmount");
						W.WriteStartAttribute("currencyID");
						W.WriteValue(MONEDA);
						W.WriteEndAttribute();
						W.WriteValue(String.Format(CultureInfo.InvariantCulture, "{0:0.00}", item.nTDeBase));
						W.WriteEndElement();

						W.WriteStartElement("cbc:TaxAmount");
						W.WriteStartAttribute("currencyID");
						W.WriteValue(MONEDA);
						W.WriteEndAttribute();
						W.WriteValue(String.Format(CultureInfo.InvariantCulture, "{0:0.00}", item.nTDeIGV));
						W.WriteEndElement();

						W.WriteStartElement("cac:TaxCategory"); //Afectación del IGV (Catálogo No. 07)

						W.WriteStartElement("cbc:Percent"); //Porcentaje de Sunat aplicado
						W.WriteValue("18");//por defecto
						W.WriteEndElement();
						W.WriteStartElement("cbc:TaxExemptionReasonCode");

						if (oEmpresa.nEmpExonerado == Enumerador.ESTADO_ACTIVO)
						{
							W.WriteValue("20");
						}
						else
						{
							W.WriteValue("11");
						}

						W.WriteEndElement();
						W.WriteStartElement("cbc:TaxScheme");

						W.WriteStartElement("cbc:ID"); //Identificación del tributo según Catálogo No. 05

						if (oEmpresa.nEmpExonerado == Enumerador.ESTADO_ACTIVO)
						{
							W.WriteValue("9997");
							W.WriteEndElement();

							W.WriteStartElement("cbc:Name"); //Identificación del tributo según Catálogo No. 05
							W.WriteValue("EXO");
							W.WriteEndElement();

							W.WriteStartElement("cbc:TaxTypeCode");
							W.WriteValue("VAT");
							W.WriteEndElement();
						}
						else
						{

							W.WriteValue("9996");
							W.WriteEndElement();

							W.WriteStartElement("cbc:Name"); //Identificación del tributo según Catálogo No. 05
							W.WriteValue("GRA");
							W.WriteEndElement();

							W.WriteStartElement("cbc:TaxTypeCode");
							W.WriteValue("FRE");
							W.WriteEndElement();//cbc:TaxTypeCode
						}

						W.WriteEndElement();//TaxScheme
					}

					#endregion
					else
					{
						// ***** Igv Total del Documento *****
						W.WriteStartElement("cac:TaxTotal");
						W.WriteStartElement("cbc:TaxAmount"); //Importe total de un tributo para este ítem
						W.WriteStartAttribute("currencyID");
						W.WriteValue(MONEDA);
						W.WriteEndAttribute();
						W.WriteValue(String.Format(CultureInfo.InvariantCulture, "{0:0.00}", item.nTDeIGV));
						W.WriteEndElement();

						// ***** Igv x Producto *****
						W.WriteStartElement("cac:TaxSubtotal");

						W.WriteStartElement("cbc:TaxableAmount");
						W.WriteStartAttribute("currencyID");
						W.WriteValue(MONEDA);
						W.WriteEndAttribute();
						W.WriteValue(String.Format(CultureInfo.InvariantCulture, "{0:0.00}", item.nTDeBase));
						W.WriteEndElement();

						W.WriteStartElement("cbc:TaxAmount");
						W.WriteStartAttribute("currencyID");
						W.WriteValue(MONEDA);
						W.WriteEndAttribute();
						W.WriteValue(String.Format(CultureInfo.InvariantCulture, "{0:0.00}", item.nTDeIGV));
						W.WriteEndElement();

						W.WriteStartElement("cac:TaxCategory");
						W.WriteStartElement("cbc:Percent"); //Porcentaje de Sunat aplicado
						W.WriteValue("18");//por defecto
						W.WriteEndElement();

						W.WriteStartElement("cbc:TaxExemptionReasonCode");

						if (oEmpresa.nEmpExonerado == Enumerador.ESTADO_ACTIVO)
						{
							W.WriteValue("20");
						}
						else
						{
							if (item.nTDeEstadoIGV == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_IGV_GRABADO)
								W.WriteValue("10");
							else
							   if (item.nTDeEstadoIGV == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_IGV_NO_GRABADO)
							{
								W.WriteValue("30"); //INAFECTO OPERACION
							}
							else //EXONERADO
							{
								W.WriteValue("20");
							}
						}

						W.WriteEndElement();

						W.WriteStartElement("cac:TaxScheme");
						W.WriteStartElement("cbc:ID"); //Identificación del tributo según Catálogo No. 05


						if (oEmpresa.nEmpExonerado == Enumerador.ESTADO_ACTIVO)
						{
							W.WriteValue("9997");
						}
						else
						if (item.nTDeEstadoIGV == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_IGV_EXONERADO)
						{
							W.WriteValue("9997");
						}
						else
							if (item.nTDeEstadoIGV ==
							(int)Enumerador.FACTURA_ELECTRONICA.TIPO_IGV_NO_GRABADO)
						{
							W.WriteValue("9998");//INAFECTO
						}
						else
						{
							W.WriteValue("1000");
						}

						W.WriteEndElement();

						W.WriteStartElement("cbc:Name"); //Nombre del Tributo (IGV, ISC)

						if (oEmpresa.nEmpExonerado == Enumerador.ESTADO_ACTIVO)
						{
							W.WriteValue("EXO");
						}
						else
						if (item.nTDeEstadoIGV == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_IGV_EXONERADO)
						{
							W.WriteValue("EXO");
						}
						else
								 if (item.nTDeEstadoIGV ==
							   (int)Enumerador.FACTURA_ELECTRONICA.TIPO_IGV_NO_GRABADO)
						{
							W.WriteValue("INA");
						}
						else
						{
							W.WriteValue("IGV");
						}

						W.WriteEndElement();

						W.WriteStartElement("cbc:TaxTypeCode"); //Código del Tipo de Tributo (UN/ECE 5153)

						if (item.nTDeEstadoIGV == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_IGV_EXONERADO)
						{
							W.WriteValue("VAT");
						}
						else
								if (item.nTDeEstadoIGV ==
							  (int)Enumerador.FACTURA_ELECTRONICA.TIPO_IGV_NO_GRABADO)
						{
							W.WriteValue("FRE");
						}
						else
						{
							W.WriteValue("VAT");
						}

						W.WriteEndElement();
						W.WriteEndElement();
					}

					W.WriteEndElement(); //cac:TaxCategory
					W.WriteEndElement(); //cac:TaxSubtotal

					//-----------------------------------------------------
					#region ICBPER
					/*en el caso de que se haya recibido este campo se debe
					 * escribir en el xml
					 * 
					*/
					if (item.nTDeICBPER > 0)
					{
						//--TaxSubtotal
						W.WriteStartElement("cac:TaxSubtotal");

						//--TaxAmount
						W.WriteStartElement("cbc:TaxAmount");
						W.WriteStartAttribute("currencyID");
						W.WriteValue(MONEDA);
						W.WriteEndAttribute();
						W.WriteValue(String.Format(CultureInfo.InvariantCulture, "{0:0.00}", item.nTDeICBPER));
						W.WriteEndElement(); //cbc:TaxAmount

						//--BaseUnitMeasure
						W.WriteStartElement("cbc:BaseUnitMeasure");
						W.WriteStartAttribute("unitCode");
						W.WriteValue(MONEDA);
						W.WriteEndAttribute();
						//OJO: La regla dice que este campo debe enviarse como entero
						//por eso la conversion
						W.WriteValue(item.nTDeCantidad.toInt());
						W.WriteEndElement(); //cbc:BaseUnitMeasure

						//--cac:TaxCategory
						W.WriteStartElement("cac:TaxCategory");


						//--cbc:PerUnitAmount
						W.WriteStartElement("cbc:PerUnitAmount");
						W.WriteStartAttribute("currencyID");
						W.WriteValue(MONEDA);
						W.WriteEndAttribute();
						//OJO: La regla dice que este campo debe enviarse como entero
						//por eso la conversion
						W.WriteValue(item.nTDeICBPER);
						W.WriteEndElement(); //cbc:PerUnitAmount


						//--cac:TaxScheme
						W.WriteStartElement("cac:TaxScheme");

						//--cbc:ID
						W.WriteStartElement("cbc:ID");
						//Identificación del tributo según Catálogo No. 05
						//codigo del ICBPER SEGUN SUNAT
						W.WriteValue("7152");
						W.WriteEndElement();//cbc:ID

						//--cbc:Name
						W.WriteStartElement("cbc:Name"); //Nombre del Tributo (IGV, ISC)
						W.WriteValue("ICBPER");
						W.WriteEndElement();//cbc:Name

						//--cbc:TaxTypeCode
						W.WriteStartElement("cbc:TaxTypeCode"); //Código del Tipo de Tributo (UN/ECE 5153)
						W.WriteValue("OTH");
						W.WriteEndElement();//cbc:TaxTypeCode

						W.WriteEndElement();//cac:TaxScheme

						W.WriteEndElement(); //cac:TaxCategory

						W.WriteEndElement(); //cac:TaxSubtotal
					}

					#endregion
					//------------------------------------------------------


					W.WriteEndElement(); //cac:TaxTotal

					W.WriteStartElement("cac:Item");
					W.WriteStartElement("cbc:Description");
					W.WriteValue(item.sProNombre);
					W.WriteEndElement();


					W.WriteStartElement("cac:SellersItemIdentification"); //Identificador de elementos de ítem
					W.WriteStartElement("cbc:ID"); //Código del producto
					W.WriteValue(item.Producto_Id);
					W.WriteEndElement();
					W.WriteEndElement();
					W.WriteEndElement(); //cac:Item
					W.WriteStartElement("cac:Price");
					W.WriteStartElement("cbc:PriceAmount");
					W.WriteStartAttribute("currencyID"); //Importe explícito a tributar Moneda e Importe total a pagar
					W.WriteValue(MONEDA);
					W.WriteEndAttribute();

					if (oTransaccion.nTraEsGratuito == Enumerador.ESTADO_ACTIVO)
						W.WriteValue("0.00");
					else
						//W.WriteValue(String.Format(CultureInfo.InvariantCulture, "{0:0.00}", item.nTDePrecio));
						if (item.nTDeEstadoIGV == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_IGV_GRABADO)
					{
						W.WriteValue(String.Format(CultureInfo.InvariantCulture, "{0:0.00}", item.nTDePrecio / (decimal)1.18));
					}
					else
					{
						W.WriteValue(String.Format(CultureInfo.InvariantCulture, "{0:0.00}", item.nTDePrecio));
					}

					W.WriteEndElement();
					W.WriteEndElement(); //cac:Price

					W.WriteEndElement(); // cac:InvoiceLine Final de Productos
				}
			}
			catch (Exception)
			{
				throw;
			}
		}

		#endregion

		#region Crear Directorio
		private void CrearDirectorio(int tipoDocumento_Id, string strNombreArchivo, Empresa oEmpresa)
		{
			try
			{

				switch (tipoDocumento_Id)
				{
					case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_FACTURA:

						if (Directory.Exists(oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_FACTURAS + strNombreArchivo))
							Array.ForEach(Directory.GetFiles(oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_FACTURAS + strNombreArchivo),
								delegate (string path)
								{
									if (path != null)
										File.Delete(path);
								});

						Directory.CreateDirectory(oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_FACTURAS + strNombreArchivo);
						break;

					case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_BOLETA:
						if (Directory.Exists(oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_BOLETAS + strNombreArchivo))
							Array.ForEach(Directory.GetFiles(oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_BOLETAS + strNombreArchivo),
								delegate (string path)
								{
									if (path != null)
										File.Delete(path);
								});
						Directory.CreateDirectory(oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_BOLETAS + strNombreArchivo);
						break;

					case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_NOTA_CREDITO:

						if (Directory.Exists(oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_NOTA_CREDITO + strNombreArchivo))
							Array.ForEach(Directory.GetFiles(oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_NOTA_CREDITO + strNombreArchivo),
								delegate (string path)
								{
									if (path != null)
										File.Delete(path);
								});
						Directory.CreateDirectory(oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_NOTA_CREDITO + strNombreArchivo);
						break;

					case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_NOTA_DEBITO:

						if (Directory.Exists(oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_NOTA_DEBITO + strNombreArchivo))
							Array.ForEach(Directory.GetFiles(oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_NOTA_DEBITO + strNombreArchivo),
								delegate (string path)
								{
									if (path != null)
										File.Delete(path);
								});

						Directory.CreateDirectory(oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_NOTA_DEBITO + strNombreArchivo);
						break;

					case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_GUIA_REMISION:

						if (Directory.Exists(oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_GUIA_REMISION + strNombreArchivo))
							Array.ForEach(Directory.GetFiles(oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_GUIA_REMISION + strNombreArchivo),
								delegate (string path)
								{
									if (path != null)
										File.Delete(path);
								});

						Directory.CreateDirectory(oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_GUIA_REMISION + strNombreArchivo);
						break;


					case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_LIQUIDACION_COMPRA:

						if (Directory.Exists(oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_LIQUIDACION_COMPRA + strNombreArchivo))
							Array.ForEach(Directory.GetFiles(oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_LIQUIDACION_COMPRA + strNombreArchivo),
								delegate (string path)
								{
									if (path != null)
										File.Delete(path);
								});

						Directory.CreateDirectory(oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_LIQUIDACION_COMPRA + strNombreArchivo);
						break;

				}
			}
			catch (Exception)
			{
				throw;
			}
		}

		private void CrearDirectorioQR(int tipoDocumento_Id, string strNombreArchivo, Empresa oEmpresa)
		{
			try
			{

				switch (tipoDocumento_Id)
				{
					case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_FACTURA:

						if (Directory.Exists(@""+oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_FACTURAS + strNombreArchivo))
							Array.ForEach(Directory.GetFiles(@"" + oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_FACTURAS + strNombreArchivo),
								delegate (string path)
								{
									if (path != null)
										if (path.Contains(".bmp"))
										{
											try
											{
												File.Delete(path);
											}
											catch
											{
												//
											}
										}
								});

						Directory.CreateDirectory(@"" + oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_FACTURAS + strNombreArchivo);
						break;

					case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_BOLETA:
						if (Directory.Exists(@"" + oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_BOLETAS + strNombreArchivo))
							Array.ForEach(Directory.GetFiles(@"" + oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_BOLETAS + strNombreArchivo),
								delegate (string path)
								{
									if (path != null)
										if (path.Contains(".bmp"))
										{
											try
											{
												File.Delete(path);
											}
											catch
											{
												//
											}
										}
								});
						Directory.CreateDirectory(@"" + oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_BOLETAS + strNombreArchivo);
						break;

					case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_NOTA_CREDITO:

						if (Directory.Exists(@"" + oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_NOTA_CREDITO + strNombreArchivo))
							Array.ForEach(Directory.GetFiles(@"" + oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_NOTA_CREDITO + strNombreArchivo),
								delegate (string path)
								{
									if (path != null)
										if (path.Contains(".bmp"))
										{
											try
											{
												File.Delete(path);
											}
											catch
											{
												//
											}

										}
								});
						Directory.CreateDirectory(@"" + oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_NOTA_CREDITO + strNombreArchivo);
						break;

					case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_NOTA_DEBITO:

						if (Directory.Exists(@"" + oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_NOTA_DEBITO + strNombreArchivo))
							Array.ForEach(Directory.GetFiles(@"" + oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_NOTA_DEBITO + strNombreArchivo),
								delegate (string path)
								{
									if (path != null)
										if (path.Contains(".bmp"))
										{
											try
											{
												File.Delete(path);
											}
											catch
											{
												//
											}
										}
								});

						Directory.CreateDirectory(@"" + oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_NOTA_DEBITO + strNombreArchivo);
						break;

					case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_GUIA_REMISION:

						if (Directory.Exists(@"" + oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_GUIA_REMISION + strNombreArchivo))
							Array.ForEach(Directory.GetFiles(@"" + oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_GUIA_REMISION + strNombreArchivo),
								delegate (string path)
								{
									if (path != null)
										if (path.Contains(".bmp"))
										{
											try
											{
												File.Delete(path);
											}
											catch
											{
												//
											}
										}
								});

						Directory.CreateDirectory(@"" + oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_GUIA_REMISION + strNombreArchivo);
						break;
				}
			}
			catch (Exception)
			{
				throw;
			}
		}
		#endregion

		#region Certificado Electronico

		public void Certificado(string strNombreArchivo, int TipoDocumento_Id, Empresa oEmpresa, int nEsBoleta = 0, int nEsNotaCfreditoBoleta = 0)
		{
			try
			{
				XmlNode root;
				XmlNode nivel1;
				XmlNode nivel2;
				XmlNode nivel3;
				string sRUTAArchivo = string.Empty;

				string certificatePath = @""+oEmpresa.sEmpRuta + RUTA_SERVIDOR_DOCUMENTO_ELECTRONICO_CERTIFICADO_RUTA + oEmpresa.sEmpNombreCertificado + ".pfx";

				LogApplicationNeg.Instance.GuardarLogAplicacion(null, "ruta certificado" + certificatePath);
				LogApplicationNeg.Instance.GuardarLogAplicacion(null, "clave certificado" + oEmpresa.sEmpClaveCertificadoFE);

				LogApplicationNeg.Instance.GuardarLogAplicacion(null, "Cargando el certificado...");

				X509Certificate2 cert = new X509Certificate2(certificatePath, oEmpresa.sEmpClaveCertificadoFE, 
					X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);
				LogApplicationNeg.Instance.GuardarLogAplicacion(null, "El certificado ha sido Cargado!");


				var xmlDoc = new XmlDocument();
				xmlDoc.PreserveWhitespace = true;

				#region Seleccionar Ruta segun el Tipo de Documento

				switch (TipoDocumento_Id)
				{
					case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_FACTURA:
						sRUTAArchivo = oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_FACTURAS + strNombreArchivo + @"\" +
									   strNombreArchivo + ".xml";
						break;

					case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_BOLETA:

						sRUTAArchivo = oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_BOLETAS +
										 @"\" + strNombreArchivo + ".xml";

						if (nEsBoleta == 1)
						{
							sRUTAArchivo = oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_BOLETAS + strNombreArchivo +
										 @"\" + strNombreArchivo + ".xml";
						}

						break;

					case 0://para el caso del tratamiento del codigo hash de las boletas de forma unitaria
						sRUTAArchivo = oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_BOLETAS +
										strNombreArchivo + @"\" + strNombreArchivo + ".xml";
						break;

					case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_NOTA_CREDITO:
						sRUTAArchivo = oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_NOTA_CREDITO + strNombreArchivo + @"\" +
									  strNombreArchivo + ".xml";

						if (nEsNotaCfreditoBoleta == 1)
						{
							sRUTAArchivo = oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_BOLETAS + strNombreArchivo + ".xml";
						}


						break;

					case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_NOTA_DEBITO:
						sRUTAArchivo = oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_NOTA_DEBITO + strNombreArchivo + @"\" +
									strNombreArchivo + ".xml";
						break;

					case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_GUIA_REMISION:
						sRUTAArchivo = oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_GUIA_REMISION + strNombreArchivo + @"\" +
									strNombreArchivo + ".xml";
						break;


					case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_LIQUIDACION_COMPRA:
						sRUTAArchivo = oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_LIQUIDACION_COMPRA + strNombreArchivo + @"\" +
									strNombreArchivo + ".xml";
						break;

				}

				#endregion

				LogApplicationNeg.Instance.GuardarLogAplicacion(null, "Cargando el archivo xml...");
				xmlDoc.Load(sRUTAArchivo);
				LogApplicationNeg.Instance.GuardarLogAplicacion(null, "Archivo xml esta cargado...ejecutando tareas");

				var signedXml = new SignedXml(xmlDoc);
				//signedXml.SigningKey = cert.PrivateKey;
				signedXml.SigningKey = cert.GetRSAPrivateKey();
				LogApplicationNeg.Instance.GuardarLogAplicacion(null, "Clave key SigningKey");


				var keyInfo = new KeyInfo();
				var reference = new Reference();
				reference.Uri = "";

				var env = new XmlDsigEnvelopedSignatureTransform();
				reference.AddTransform(env);

				signedXml.AddReference(reference);
				signedXml.Signature.Id = "SignatureSP";

				var X509Chain = new X509Chain();
				X509Chain.Build(cert);

				var local_element = X509Chain.ChainElements[0];
				var keyInfoData = new KeyInfoX509Data(cert);
				string subjectName = local_element.Certificate.Subject;
				keyInfoData.AddSubjectName(subjectName);
				keyInfo.AddClause(keyInfoData);
				signedXml.KeyInfo = keyInfo;
				signedXml.ComputeSignature();

				var xmlDigitalSignature = signedXml.GetXml();
				xmlDigitalSignature.Prefix = "ds";
				signedXml.ComputeSignature();

				root = xmlDoc.DocumentElement;
				nivel1 = root.FirstChild.NextSibling;
				nivel2 = nivel1.FirstChild.NextSibling;
				nivel3 = nivel2.FirstChild.NextSibling;
				nivel3.AppendChild(xmlDigitalSignature);


				LogApplicationNeg.Instance.GuardarLogAplicacion(null, "Guardando el archivo xml en: " + sRUTAArchivo);
				//Volver a guardar el Documento
				xmlDoc.Save(sRUTAArchivo);
			}
			catch (Exception ex)
			{
				LogApplicationNeg.Instance.GuardarLogAplicacion(ex,"Catch");
				throw;
			}
		}

		#endregion

		#region Comprimir XML

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sNombreFactura"></param>
		/// <param name="TipoDocumento_Id"></param>
		/// <param name="oEmpresa"></param>
		private void ComprimirXML(string sNombreFactura, int TipoDocumento_Id, Empresa oEmpresa)
		{
			string zipPath = string.Empty;
			string sRUTAArchivo = string.Empty;

			switch (TipoDocumento_Id)
			{
				case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_FACTURA:
					sRUTAArchivo = oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_FACTURAS + strNombreArchivo + @"\" +
								   strNombreArchivo + ".xml";

					zipPath = oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_FACTURAS + sNombreFactura + @"\" + sNombreFactura + ".zip";

					break;

				case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_BOLETA:
					sRUTAArchivo = oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_BOLETAS + strNombreArchivo + @"\" +
								   strNombreArchivo + ".xml";

					zipPath = oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_BOLETAS + sNombreFactura + @"\" + sNombreFactura + ".zip";

					break;

				case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_NOTA_CREDITO:
					sRUTAArchivo = oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_NOTA_CREDITO + strNombreArchivo + @"\" +
								  strNombreArchivo + ".xml";
					zipPath = oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_NOTA_CREDITO + sNombreFactura + @"\" + sNombreFactura + ".zip";
					break;

				case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_NOTA_DEBITO:
					sRUTAArchivo = oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_NOTA_DEBITO + strNombreArchivo + @"\" +
								strNombreArchivo + ".xml";
					zipPath = oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_NOTA_DEBITO + sNombreFactura + @"\" + sNombreFactura + ".zip";
					break;

				case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_GUIA_REMISION:
					sRUTAArchivo = oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_GUIA_REMISION + strNombreArchivo + @"\" +
								strNombreArchivo + ".xml";
					zipPath = oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_GUIA_REMISION + sNombreFactura + @"\" + sNombreFactura + ".zip";
					break;


				case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_LIQUIDACION_COMPRA:
					sRUTAArchivo = oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_LIQUIDACION_COMPRA + strNombreArchivo + @"\" +
								strNombreArchivo + ".xml";
					zipPath = oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_LIQUIDACION_COMPRA + sNombreFactura + @"\" + sNombreFactura + ".zip";
					break;

			}

			try
			{
				var zip = new Ionic.Zip.ZipFile();
				zip.AddFile(sRUTAArchivo, string.Empty);
				zip.Save(zipPath);
			}
			catch
			{
				//ignorar compresion: siempre se comprime y no afecta al proceso
			}
		}

		#endregion

		//Baja se Archivos Sunat
		#region Generar Archivo XML DE BAJAS

		/// <summary>
		/// 
		/// </summary>
		/// <param name="W"></param>
		/// <param name="sNombre_Doc_RA"></param>
		/// <param name="sFechaCabecera"></param>
		/// <param name="oTransaccion"></param>
		/// <param name="p_oEmpresa"></param>
		protected static void GenerarArchivoXmlBajas(XmlTextWriter W, string sNombre_Doc_RA, string sFechaCabecera,
													 Transaccion oTransaccion, Empresa p_oEmpresa)
		{
			try
			{
				#region VARIABLES

				string strDescripcionAnulacion = string.Empty;
				string CODIGO_DOCUMENTO_SUNAT = string.Empty;

				#endregion

				W.WriteStartDocument(false);

				#region INICIO DEL DOCUMENTO XML

				W.WriteStartElement("VoidedDocuments"); //  Documento utilizado para requerir un pago
				W.WriteStartAttribute("xmlns");
				W.WriteValue("urn:sunat:names:specification:ubl:peru:schema:xsd:VoidedDocuments-1");

				W.WriteStartAttribute("xmlns:cac");
				W.WriteValue("urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");

				W.WriteStartAttribute("xmlns:cbc");
				W.WriteValue("urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");

				W.WriteStartAttribute("xmlns:ds");
				W.WriteValue("http://www.w3.org/2000/09/xmldsig#");

				W.WriteStartAttribute("xmlns:ext");
				W.WriteValue("urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2");

				W.WriteStartAttribute("xmlns:qdt");
				W.WriteValue("urn:oasis:names:specification:ubl:schema:xsd:QualifiedDatatypes-2");

				W.WriteStartAttribute("xmlns:sac");
				W.WriteValue("urn:sunat:names:specification:ubl:peru:schema:xsd:SunatAggregateComponents-1");

				#endregion

				#region CUERPO

				W.WriteStartAttribute("xmlns:xsi");
				W.WriteValue("http://www.w3.org/2001/XMLSchema-instance");
				W.WriteEndAttribute();
				W.WriteStartElement("ext:UBLExtensions");
				//  Contenedor de Componentes de extensión. Podrán incorporarse nuevas definiciones estructuradas cuando sean de interés conjunto para emisores y receptores, y no estén ya definidas en el esquema de la factura. 
				W.WriteStartElement("ext:UBLExtension");
				W.WriteStartElement("ext:ExtensionContent");
				W.WriteEndElement();
				W.WriteEndElement();
				W.WriteEndElement(); //UBLExtensions
				W.WriteStartElement("cbc:UBLVersionID"); //Versión del UBL
				W.WriteValue("2.0");
				W.WriteEndElement();
				W.WriteStartElement("cbc:CustomizationID"); //Versión de la estructura del documento
				W.WriteValue("1.0");
				W.WriteEndElement();
				W.WriteStartElement("cbc:ID");
				//ESTE NOMBRE SE RECIBE COMO PARAMETRO Y ES FORMADO EN EL METODO DE LLAMADA SEGÚN SUNAT
				W.WriteValue(sNombre_Doc_RA);
				W.WriteEndElement();

				#endregion

				#region FECHA EMISION DEL DOCUMENTO

				W.WriteStartElement("cbc:ReferenceDate");
				//Fecha de emisión del documento (yyyy-mm-dd)
				W.WriteValue(sFechaCabecera);
				W.WriteEndElement();

				#endregion

				#region FECHA EMISION DEL DOCUMENTO II

				W.WriteStartElement("cbc:IssueDate");
				// W.WriteValue("2015-06-12");     //Fecha de emisión del documento (yyyy-mm-dd)
				W.WriteValue(sFechaCabecera);
				W.WriteEndElement();

				#endregion

				#region ESPACIO PARA CERTIFICADO

				// ***** Firma Digital *****
				W.WriteStartElement("cac:Signature"); //Referencia a la Firma Digital
				W.WriteStartElement("cbc:ID"); //Identificador de la firma
				W.WriteValue("IDSignSP");
				W.WriteEndElement();

				// ***** Datos Empresa certificado ******
				W.WriteStartElement("cac:SignatoryParty"); //Código del tipo de documento adicional (p.e. SCOP)
				W.WriteStartElement("cac:PartyIdentification"); //Parte firmante
				W.WriteStartElement("cbc:ID"); //Identificación de la parte firmante
				W.WriteValue(p_oEmpresa.sEmpRuc);
				W.WriteEndElement();
				W.WriteEndElement();

				W.WriteStartElement("cac:PartyName");
				W.WriteStartElement("cbc:Name"); //Nombre de la parte firmante
				W.WriteValue(p_oEmpresa.sEmpNombre);
				W.WriteEndElement();
				W.WriteEndElement();
				W.WriteEndElement();

				W.WriteStartElement("cac:DigitalSignatureAttachment");
				//Asociación con la firma codificada (en formato XMLDSIG, por ejemplo)
				W.WriteStartElement("cac:ExternalReference");
				//Información acerca de un documento vinculado. Los vínculos pueden ser externos (referenciados mediante un elemento URI), internos (accesibles mediante un elemento MIME) o pueden estar contenidos dentro del mismo documento en el que se alude a ellos (mediante elementos Documento Incrustado)
				W.WriteStartElement("cbc:URI");
				//Identificador de Recurso Uniforme (o URI) que identifica la localización de la firma
				W.WriteValue("#SignatureSP");
				W.WriteEndElement();
				W.WriteEndElement();
				W.WriteEndElement();
				W.WriteEndElement(); //cac:Signature

				#endregion

				#region DATOS DE LA EMPRESA

				W.WriteStartElement("cac:AccountingSupplierParty"); //Datos del Emisor del documento
				W.WriteStartElement("cbc:CustomerAssignedAccountID");
				//Número de documento de identidad (“Número de RUC”)
				W.WriteValue(p_oEmpresa.sEmpRuc);
				W.WriteEndElement();

				W.WriteStartElement("cbc:AdditionalAccountID"); //Tipo de documento de identificación
				W.WriteValue(p_oEmpresa.sEmpTipoIdentificadorEmpresaSunat);
				W.WriteEndElement();

				W.WriteStartElement("cac:Party");
				W.WriteStartElement("cac:PartyLegalEntity");
				W.WriteStartElement("cbc:RegistrationName"); // Apellidos y nombres o denominación o razón social
				W.WriteValue(p_oEmpresa.sEmpNombre);
				W.WriteEndElement();
				W.WriteEndElement();
				W.WriteEndElement(); //cac:Party         
				W.WriteEndElement(); //cac:AccountingSupplierParty

				#endregion

				#region CREAR DETALLE DE ANULACION SEGÚN TIPO DE DOC

				W.WriteStartElement("sac:VoidedDocumentsLine");
				W.WriteStartElement("cbc:LineID");
				W.WriteValue("1");
				W.WriteEndElement();
				W.WriteStartElement("cbc:DocumentTypeCode");

				#region TIPO DE DOCUMENTO SUNAT

				switch (oTransaccion.TipoDocumento_Id)
				{
					#region 02 - Factura - > sunat 01

					case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_FACTURA:

						CODIGO_DOCUMENTO_SUNAT = "01";
						strDescripcionAnulacion = "ANULACIÓN DE FACTURA 1";
						break;

					#endregion

					#region 01 - Boleta - > sunat 03

					case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_BOLETA:
						CODIGO_DOCUMENTO_SUNAT = "03";
						strDescripcionAnulacion = "ANULACIÓN DE BOLETA 1";
						break;

					#endregion

					#region 19 - Nota de Crédito - > sunat 07

					case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_NOTA_CREDITO:
						CODIGO_DOCUMENTO_SUNAT = "07";
						strDescripcionAnulacion = "ANULACIÓN DE NOTA DE CREDITO 1";
						break;

					#endregion

					#region 49 - Nota de Debito - > sunat 08

					case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_NOTA_DEBITO:
						CODIGO_DOCUMENTO_SUNAT = "08";
						strDescripcionAnulacion = "ANULACIÓN DE NOTA DE DEBITO 1";
						break;

					#endregion

					#region 49 - Guia de Remision - > sunat 09

					case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_GUIA_REMISION:
						CODIGO_DOCUMENTO_SUNAT = "09";
						strDescripcionAnulacion = "ANULACIÓN DE GUIA DE REMISION 1";
						break;

						#endregion
				}

				#endregion


				W.WriteValue(CODIGO_DOCUMENTO_SUNAT);
				W.WriteEndElement();
				W.WriteStartElement("sac:DocumentSerialID");
				W.WriteValue(oTransaccion.sTraSerie);
				W.WriteEndElement();
				W.WriteStartElement("sac:DocumentNumberID");
				W.WriteValue(oTransaccion.sTraNumero);
				W.WriteEndElement();
				W.WriteStartElement("sac:VoidReasonDescription");
				W.WriteValue(strDescripcionAnulacion);
				W.WriteEndElement();

				W.WriteEndElement();

				#endregion

				#region FIN ESCRITURA DEL XML

				W.Flush();
				W.Close();

				#endregion
			}
			catch (Exception)
			{
				throw;
			}
		}

		#endregion

		#region Certificado Electronico para Bajas

		/// <summary>
		/// Incrusta el certificado en una parte del xml
		/// para los archivos de bajas
		/// </summary>
		/// <param name="Ruta"></param>
		/// <param name="oEmpresa"></param>
		public void CertificadoBajas(string Ruta, Empresa oEmpresa)
		{
			try
			{
				XmlNode root;
				XmlNode nivel1;
				XmlNode nivel2;
				XmlNode nivel3;

				string certificatePath = @""+oEmpresa.sEmpRuta + RUTA_SERVIDOR_DOCUMENTO_ELECTRONICO_CERTIFICADO_RUTA + oEmpresa.sEmpNombreCertificado + ".pfx";
				//var cert = new X509Certificate2(certificatePath, oEmpresa.sEmpClaveCertificadoFE);


				X509Certificate2 cert = new X509Certificate2(certificatePath, oEmpresa.sEmpClaveCertificadoFE,
					X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);
				LogApplicationNeg.Instance.GuardarLogAplicacion(null, "El certificado ha sido Cargado!");



				var xmlDoc = new XmlDocument();
				xmlDoc.PreserveWhitespace = true;

				xmlDoc.Load(Ruta + ".xml");

				var signedXml = new SignedXml(xmlDoc);

				//signedXml.SigningKey = cert.PrivateKey;
				signedXml.SigningKey = cert.GetRSAPrivateKey();
				var keyInfo = new KeyInfo();
				var reference = new Reference();
				reference.Uri = "";

				var env = new XmlDsigEnvelopedSignatureTransform();
				reference.AddTransform(env);

				signedXml.AddReference(reference);
				signedXml.Signature.Id = "SignatureSP";

				var X509Chain = new X509Chain();
				X509Chain.Build(cert);

				var local_element = X509Chain.ChainElements[0];
				var keyInfoData = new KeyInfoX509Data(cert);
				string subjectName = local_element.Certificate.Subject;
				keyInfoData.AddSubjectName(subjectName);
				keyInfo.AddClause(keyInfoData);
				signedXml.KeyInfo = keyInfo;
				signedXml.ComputeSignature();

				var xmlDigitalSignature = signedXml.GetXml();
				xmlDigitalSignature.Prefix = "ds";
				signedXml.ComputeSignature();

				root = xmlDoc.DocumentElement;
				nivel1 = root.FirstChild.NextSibling;
				nivel2 = nivel1.FirstChild.NextSibling;
				nivel3 = nivel2.FirstChild.NextSibling;

				nivel3.AppendChild(xmlDigitalSignature);
				xmlDoc.Save(Ruta + ".xml");
			}
			catch (Exception)
			{
				throw;
			}
		}

		#endregion

		#region Guardar y Comprimir xml

		/// <summary>
		/// crear el archivo de la  ruta y la convierte
		/// en la misma ruta en un archivo .zip
		/// </summary>
		/// <param name="Ruta"></param>
		protected static void GuardaryComprimirXML(string Ruta)
		{
			try
			{
				var zip = new Ionic.Zip.ZipFile();
				zip.AddFile(Ruta + ".xml", string.Empty);
				zip.Save(Ruta + ".zip");
			}
			catch (Exception)
			{
				throw;
			}
		}

		#endregion

		//Resumen Boletas Electronicas

		#region Generar Archivo XML Resumen Boletas Electronicas

		/// <summary>
		/// Crea el archivo xml y devuelve la ruta de creación
		/// incluye el certificado
		/// </summary>
		/// <param name="LstTraBoletas"></param>
		/// <param name="oEmpresa"></param>
		/// <param name="status"></param>
		/// <returns></returns>
		public string GenerarArchivoXmlyZipResumenBoletas(List<Transaccion> LstTraBoletas, Empresa oEmpresa, string status = "1", int nEsNotaCreditoBoleta = 0)
		{
			try
			{
				#region Variables

				string sFechaCabecera = string.Empty;
				string sFechaFormato = string.Empty;
				strNombreArchivo = string.Empty;
				string strRuta = string.Empty;
				//para ser llenado en el xml
				string sNombre_Doc_RA = string.Empty;
				sFechaFormato = DateTime.Now.Date.ToString("yyyyMMdd");
				sFechaCabecera = LstTraBoletas.FirstOrDefault().dTraFecha.ToString("yyyy-MM-dd");

				string strDescripcionAnulacion_correo = string.Empty;

				//ordenar la lista xsiak xd
				var ListaOrdenada = LstTraBoletas.OrderBy(x => x.sTraNumero).ToList();
				//obtener los desde hasta
				var oBoletaInicial = ListaOrdenada.FirstOrDefault();
				var oBoletaFinal = ListaOrdenada.LastOrDefault();

				#endregion

				#region Obtener Correlativo para nombrar al archivo xml

				var Parametros = new Dictionary<string, object>();
				Parametros.Add("Empresa_Id", oEmpresa.Empresa_Id);
				Parametros.Add("TipoDocumento_Id", (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_CORRELATIVO_DOC_ELECTRONICO_BOLETA_SUNAT);

				var Lista = TicketNeg.Instance.Consultar(Parametros);
				//el nuevo numero correlativo aumentado en 1

				if (Lista == null || Lista.Count <= 0) throw new Exception("No se Encuentra el Correlativo de BOLETAS");

				var oTicket = Lista.FirstOrDefault();
				int nCorrelativoBoletas = oTicket.nTicUltimoNumero;
				nCorrelativoBoletas = nCorrelativoBoletas + 1;

				#endregion

				#region Nombre del archivo de Resumenes de Boletas Electronicas

				//formar el nombre del archivo
				strNombreArchivo = oEmpresa.sEmpRuc + "-RC-" + sFechaFormato + "-" + nCorrelativoBoletas;

				//exclusivo para el documento xml se necesita en una parte
				sNombre_Doc_RA = "RC-" + sFechaFormato + "-" + nCorrelativoBoletas;

				#endregion

				#region Creacion de la ruta del archivo

				strRuta = RUTA_REPOSITORIO_ELECTRONICO_BOLETAS + strNombreArchivo;

				#endregion

				#region CREAR XML

				#region Instancia del escritor XML

				var Wrt = new XmlTextWriter(oEmpresa.sEmpRuta + strRuta + ".xml", Encoding.GetEncoding("ISO-8859-1"));
				Wrt.Formatting = Formatting.Indented;

				#endregion

				#region Cabecera Resumen Boleta Eletronica

				Generar_CabeceraResumenBoletaXML(Wrt, sNombre_Doc_RA, sFechaCabecera, oEmpresa);

				#endregion

				#region Detalle del Resumen de Boletas

				int i = 1;
				foreach (var oTraBoleta in LstTraBoletas)
				{
					Generar_DetalleResumenBoletaXML_II(oTraBoleta, Wrt, i, status);
					i++;
				}

				#endregion

				#region Final Xml

				Wrt.WriteEndElement(); // Cerrando Nodos Principales
				Wrt.WriteEndDocument(); // Cerrando Nodos Principales

				// ***** Grabando datos del XML *****
				Wrt.Flush();
				Wrt.Close();

				#endregion

				#region  INCRUSTAR CERTIFICADO

				if (oBoletaInicial.TipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_NOTA_CREDITO)
				{
					nEsNotaCreditoBoleta = 1;
				}

				Certificado(strNombreArchivo, oBoletaInicial.TipoDocumento_Id, oEmpresa, 0, nEsNotaCreditoBoleta);

				#endregion

				#endregion

				#region Actualizar Correlativo

				/*Nota: Si sucede algun error no hay problema si se envia el correlativo siguiente */
				TicketNeg.Instance.AumentarCorrelativo(oTicket.Ticket_Id, oTicket.nTicUltimoNumero);

				#endregion

				return strNombreArchivo;
			}
			catch (Exception)
			{
				throw;
			}
		}

		#endregion

		#region Resumen de Boletas cabecera

		private void Generar_CabeceraResumenBoletaXML(XmlTextWriter W, string sNombre_Doc_RA, string sFechaCabecera,
			Empresa p_oEmpresa)
		{
			try
			{
				// ***** Iniciando el XML *****        
				W.WriteStartDocument(false);

				W.WriteStartElement("SummaryDocuments"); //  summatoria de boleta de venta
				W.WriteStartAttribute("xmlns");
				W.WriteValue("urn:sunat:names:specification:ubl:peru:schema:xsd:SummaryDocuments-1");

				W.WriteStartAttribute("xmlns:cac");
				W.WriteValue("urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");

				W.WriteStartAttribute("xmlns:cbc");
				W.WriteValue("urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");

				//W.WriteStartAttribute("xmlns:ccts");
				//W.WriteValue("urn:un:unece:uncefact:documentation:2");

				W.WriteStartAttribute("xmlns:ds");
				W.WriteValue("http://www.w3.org/2000/09/xmldsig#");

				W.WriteStartAttribute("xmlns:ext");
				W.WriteValue("urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2");

				//W.WriteStartAttribute("xmlns:qdt");
				//W.WriteValue("urn:oasis:names:specification:ubl:schema:xsd:QualifiedDatatypes-2");

				W.WriteStartAttribute("xmlns:sac");
				W.WriteValue("urn:sunat:names:specification:ubl:peru:schema:xsd:SunatAggregateComponents-1");

				//W.WriteStartAttribute("xmlns:udt");
				//W.WriteValue("urn:un:unece:uncefact:data:specification:UnqualifiedDataTypesSchemaModule:2");

				W.WriteStartAttribute("xmlns:xsi");
				W.WriteValue("http://www.w3.org/2001/XMLSchema-instance");

				W.WriteEndAttribute();

				W.WriteStartElement("ext:UBLExtensions");
				//  Contenedor de Componentes de extensión. Podrán incorporarse nuevas definiciones estructuradas cuando sean de interés conjunto para emisores y receptores, y no estén ya definidas en el esquema de la factura. 
				W.WriteStartElement("ext:UBLExtension");
				W.WriteStartElement("ext:ExtensionContent");

				W.WriteEndElement();
				W.WriteEndElement();
				W.WriteEndElement();

				W.WriteStartElement("cbc:UBLVersionID"); //Versión del UBL
				W.WriteValue("2.0");
				W.WriteEndElement();

				W.WriteStartElement("cbc:CustomizationID"); //Versión de la estructura del documento
				W.WriteValue("1.1");
				W.WriteEndElement();

				W.WriteStartElement("cbc:ID");
				//ESTE NOMBRE SE RECIBE COMO PARAMETRO Y ES FORMADO EN EL METODO DE LLAMADA SEGÚN SUNAT
				W.WriteValue(sNombre_Doc_RA); //Identificador del resumen           
				W.WriteEndElement();

				W.WriteStartElement("cbc:ReferenceDate");
				W.WriteValue(sFechaCabecera);
				//Fecha de emisión de los documentos contenidos en el documento resumen
				W.WriteEndElement();

				W.WriteStartElement("cbc:IssueDate");
				W.WriteValue(DateTime.Now.ToString("yyyy-MM-dd"));
				//Fecha de generación del documento resumen
				W.WriteEndElement();

				//W.WriteStartElement("cbc:ReferenceDate");
				//W.WriteValue("2015-12-31");                 //Fecha de emisión de los documentos contenidos en el documento resumen
				//W.WriteEndElement();

				//W.WriteStartElement("cbc:IssueDate");
				//W.WriteValue("2015-12-31");                //Fecha de generación del documento resumen
				//W.WriteEndElement();

				// ***** Firma Digital *****
				W.WriteStartElement("cac:Signature"); //Referencia a la Firma Digital
				W.WriteStartElement("cbc:ID"); //Identificador de la firma
				W.WriteValue("IDSignSP");
				W.WriteEndElement();

				// ***** Datos Empresa certificado ******
				W.WriteStartElement("cac:SignatoryParty"); //Código del tipo de documento adicional (p.e. SCOP)
				W.WriteStartElement("cac:PartyIdentification"); //Parte firmante
				W.WriteStartElement("cbc:ID"); //Identificación de la parte firmante
											   //W.WriteValue("20100127912");
				W.WriteValue(p_oEmpresa.sEmpRuc);
				W.WriteEndElement();
				W.WriteEndElement();

				W.WriteStartElement("cac:PartyName");
				W.WriteStartElement("cbc:Name"); //Nombre de la parte firmante
				W.WriteValue(p_oEmpresa.sEmpNombre);
				W.WriteEndElement();
				W.WriteEndElement();
				W.WriteEndElement();


				W.WriteStartElement("cac:DigitalSignatureAttachment");
				//Asociación con la firma codificada (en formato XMLDSIG, por ejemplo)
				W.WriteStartElement("cac:ExternalReference");
				//Información acerca de un documento vinculado. Los vínculos pueden ser externos (referenciados mediante un elemento URI), internos (accesibles mediante un elemento MIME) o pueden estar contenidos dentro del mismo documento en el que se alude a ellos (mediante elementos Documento Incrustado)
				W.WriteStartElement("cbc:URI");
				//Identificador de Recurso Uniforme (o URI) que identifica la localización de la firma
				W.WriteValue("#SignatureSP");
				W.WriteEndElement();
				W.WriteEndElement();
				W.WriteEndElement();
				W.WriteEndElement(); //cac:Signature

				// ***** Empresa *****
				W.WriteStartElement("cac:AccountingSupplierParty"); //Datos del Emisor del documento
				W.WriteStartElement("cbc:CustomerAssignedAccountID");
				//Número de documento de identidad (“Número de RUC”)
				W.WriteValue(p_oEmpresa.sEmpRuc);
				W.WriteEndElement();

				W.WriteStartElement("cbc:AdditionalAccountID"); //Tipo de documento de identificación
																//este numero aparece en el xml sin cero y se probara enviando el 06 que viene dsde la consulta a empresa
				W.WriteValue("6");
				W.WriteEndElement();

				W.WriteStartElement("cac:Party");
				W.WriteStartElement("cac:PartyLegalEntity");
				W.WriteStartElement("cbc:RegistrationName");
				// Apellidos y nombres o denominación o razón social
				W.WriteValue(p_oEmpresa.sEmpNombre);
				W.WriteEndElement();
				W.WriteEndElement();
				W.WriteEndElement(); //cac:Party         

				W.WriteEndElement(); //cac:AccountingSupplierParty
			}
			catch (Exception)
			{
				throw;
			}
		}

		#endregion

		#region Resumen de Detalle de Boletas II

		/// <summary>
		/// 
		/// </summary>
		/// <param name="oTraBoleta"></param>
		/// <param name="W"></param>
		/// <param name="Linea_Id"></param>
		/// <param name="status"></param>
		private void Generar_DetalleResumenBoletaXML_II(Transaccion oTraBoleta, XmlTextWriter W, int Linea_Id, string status)
		{
			try
			{

				string MONEDA = oTraBoleta.Moneda_Id == Enumerador.MONEDA_NACIONAL_SOLES_ID ? "PEN" : "USD";

				#region <sac:SummaryDocumentsLine>

				W.WriteStartElement("sac:SummaryDocumentsLine"); //Ítems de Factura
				W.WriteStartElement("cbc:LineID"); // Número de orden del Ítem
				W.WriteValue(Linea_Id);
				W.WriteEndElement();

				W.WriteStartElement("cbc:DocumentTypeCode"); // Tipo documento

				if (oTraBoleta.TipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_BOLETA)
					W.WriteValue("03");
				else
					if (oTraBoleta.TipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_NOTA_CREDITO)
					W.WriteValue("07");
				else
						if (oTraBoleta.TipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_NOTA_DEBITO)
					W.WriteValue("08");

				W.WriteEndElement();

				W.WriteStartElement("cbc:ID"); // serie y numero correlativo del documento
				W.WriteValue(oTraBoleta.sTraSerie + "-" + oTraBoleta.sTraNumero);
				W.WriteEndElement();

				#region cac:AccountingCustomerParty

				W.WriteStartElement("cac:AccountingCustomerParty");

				//DNI o ruc del cliente
				W.WriteStartElement("cbc:CustomerAssignedAccountID"); // Número de orden del Ítem
				W.WriteValue(oTraBoleta.sCliNroIdentidad);
				W.WriteEndElement();

				#region cbc:AdditionalAccountID  DOCUMENTODEL CLIENTE RUC O DNI

				//si es DNI es 1
				//si es RUC es 6 esto es segun el catalogo 06
				W.WriteStartElement("cbc:AdditionalAccountID"); // Número de orden del Ítem

				if (oTraBoleta.sCliNroIdentidad.Length > 8)
					W.WriteValue("6");
				else
					W.WriteValue("1");

				W.WriteEndElement();

				#endregion

				W.WriteEndElement();

				#endregion

				if (oTraBoleta.TipoDocumento_Id !=
				   (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_BOLETA)
				{
					#region Referencia de las notas de credito o debito

					W.WriteStartElement("cac:BillingReference");

					W.WriteStartElement("cac:InvoiceDocumentReference");

					W.WriteStartElement("cbc:ID");
					W.WriteValue(oTraBoleta.sSerieNumeroRelacionado);
					W.WriteEndElement();

					W.WriteStartElement("cbc:DocumentTypeCode");
					W.WriteValue("03");

					W.WriteEndElement();

					W.WriteEndElement(); //cac:InvoiceDocumentReference

					W.WriteEndElement(); //<//cac:BillingReference>

					#endregion
				}

				#region <cac:Status>

				W.WriteStartElement("cac:Status");

				W.WriteStartElement("cbc:ConditionCode");
				W.WriteValue(status);
				W.WriteEndElement();

				W.WriteEndElement(); //</cac:Status>

				#endregion </cac:Status>

				#region Importe total de la boleta

				// <!-- Importe total de la venta, cesion en uso o del servicio prestado (Precio incluye todos los impuestos) -->   
				W.WriteStartElement("sac:TotalAmount");
				W.WriteStartAttribute("currencyID");
				W.WriteValue(MONEDA);
				W.WriteEndAttribute();
				W.WriteValue(String.Format(CultureInfo.InvariantCulture, "{0:0.00}", oTraBoleta.nTraImporteTotal));
				W.WriteEndElement();

				#endregion

				#region Sumatoria de las bases que estana fectas a IGV

				if (oTraBoleta.nTraEsGratuito == Enumerador.ESTADO_ACTIVO)
				{
					decimal PaidAmounAfectaIGV = oTraBoleta.LstTransaccionDetalle.Sum(y => y.nTDeBase);
					W.WriteStartElement("sac:BillingPayment");
					W.WriteStartElement("cbc:PaidAmount");
					W.WriteStartAttribute("currencyID");
					W.WriteValue(MONEDA);
					W.WriteEndAttribute();
					W.WriteValue(String.Format(CultureInfo.InvariantCulture, "{0:0.00}", PaidAmounAfectaIGV));
					W.WriteEndElement();
					W.WriteStartElement("cbc:InstructionID");
					W.WriteValue("05");
					W.WriteEndElement();
					W.WriteEndElement(); //BillingPayment
				}
				else
				{
					// <!-- Total valor de venta - operaciones gravadas --> 

					if (oTraBoleta.LstTransaccionDetalle[0].nTDeEstadoIGV == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_IGV_GRABADO)
					{
						decimal PaidAmounAfectaIGV = oTraBoleta.LstTransaccionDetalle.Where(x => x.nTDeIGV > 0).Sum(y => y.nTDeBase);
						W.WriteStartElement("sac:BillingPayment");
						W.WriteStartElement("cbc:PaidAmount");
						W.WriteStartAttribute("currencyID");
						W.WriteValue(MONEDA);
						W.WriteEndAttribute();
						W.WriteValue(String.Format(CultureInfo.InvariantCulture, "{0:0.00}", PaidAmounAfectaIGV));
						W.WriteEndElement();
						W.WriteStartElement("cbc:InstructionID");
						W.WriteValue("01");
						W.WriteEndElement();
						W.WriteEndElement(); //BillingPayment
					}

					if (oTraBoleta.LstTransaccionDetalle[0].nTDeEstadoIGV == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_IGV_EXONERADO)
					{
						decimal PaidAmounAfectaIGV = oTraBoleta.LstTransaccionDetalle.Where(x => x.nTDeIGV == 0).Sum(y => y.nTDeBase);
						W.WriteStartElement("sac:BillingPayment");
						W.WriteStartElement("cbc:PaidAmount");
						W.WriteStartAttribute("currencyID");
						W.WriteValue(MONEDA);
						W.WriteEndAttribute();
						W.WriteValue(String.Format(CultureInfo.InvariantCulture, "{0:0.00}", PaidAmounAfectaIGV));
						W.WriteEndElement();
						W.WriteStartElement("cbc:InstructionID");
						W.WriteValue("02");
						W.WriteEndElement();
						W.WriteEndElement(); //BillingPayment
					}

					if (oTraBoleta.LstTransaccionDetalle[0].nTDeEstadoIGV == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_IGV_NO_GRABADO)
					{
						decimal PaidAmounAfectaIGV = oTraBoleta.LstTransaccionDetalle.Sum(y => y.nTDeBase);
						W.WriteStartElement("sac:BillingPayment");
						W.WriteStartElement("cbc:PaidAmount");
						W.WriteStartAttribute("currencyID");
						W.WriteValue(MONEDA);
						W.WriteEndAttribute();
						W.WriteValue(String.Format(CultureInfo.InvariantCulture, "{0:0.00}", PaidAmounAfectaIGV));
						W.WriteEndElement();
						W.WriteStartElement("cbc:InstructionID");
						W.WriteValue("03");
						W.WriteEndElement();
						W.WriteEndElement(); //BillingPayment
					}
				}

				#endregion

				#region Total IGV cac:TaxTotal Exonerado

				decimal nTotalIGV = oTraBoleta.LstTransaccionDetalle.Sum(x => x.nTDeIGV);

				//  <!-- Total IGV --> 
				W.WriteStartElement("cac:TaxTotal");
				W.WriteStartElement("cbc:TaxAmount");
				W.WriteStartAttribute("currencyID");
				W.WriteValue(MONEDA);
				W.WriteEndAttribute();

				if (oEmpresa.nEmpProduccion == Enumerador.ESTADO_INACTIVO)
					W.WriteValue(String.Format(CultureInfo.InvariantCulture, "{0:0.00}", 100)); //Prueba
				else
				{
					W.WriteValue(String.Format(CultureInfo.InvariantCulture, "{0:0.00}", 0));
				}

				W.WriteEndElement();

				#region cac:TaxSubtotal

				W.WriteStartElement("cac:TaxSubtotal");

				#region cbc:TaxAmount

				W.WriteStartElement("cbc:TaxAmount");
				W.WriteStartAttribute("currencyID");
				W.WriteValue(MONEDA);
				W.WriteEndAttribute();

				if (oEmpresa.nEmpProduccion == Enumerador.ESTADO_INACTIVO)
					W.WriteValue(String.Format(CultureInfo.InvariantCulture, "{0:0.00}", 100)); //Prueba
				else
					W.WriteValue(String.Format(CultureInfo.InvariantCulture, "{0:0.00}", 0));
				W.WriteEndElement();

				#endregion

				#region cac:TaxCategory

				W.WriteStartElement("cac:TaxCategory");

				#region cac:TaxScheme

				W.WriteStartElement("cac:TaxScheme");

				W.WriteStartElement("cbc:ID");
				W.WriteValue("2000");
				W.WriteEndElement();
				W.WriteStartElement("cbc:Name");
				W.WriteValue("ISC");
				W.WriteEndElement();
				W.WriteStartElement("cbc:TaxTypeCode");
				W.WriteValue("EXC");
				W.WriteEndElement();
				W.WriteEndElement();

				#endregion

				W.WriteEndElement(); //TaxCategory

				#endregion

				W.WriteEndElement(); //TaxSubtotal                    

				#endregion

				W.WriteEndElement(); //TaxTotal

				#endregion

				#region cac:TaxTotal Total IGV

				W.WriteStartElement("cac:TaxTotal");
				W.WriteStartElement("cbc:TaxAmount");
				W.WriteStartAttribute("currencyID");
				W.WriteValue(MONEDA);
				W.WriteEndAttribute();
				W.WriteValue(String.Format(CultureInfo.InvariantCulture, "{0:0.00}", nTotalIGV));
				W.WriteEndElement();
				W.WriteStartElement("cac:TaxSubtotal");
				W.WriteStartElement("cbc:TaxAmount");
				W.WriteStartAttribute("currencyID");
				W.WriteValue(MONEDA);
				W.WriteEndAttribute();
				W.WriteValue(String.Format(CultureInfo.InvariantCulture, "{0:0.00}", nTotalIGV));
				W.WriteEndElement();
				W.WriteStartElement("cac:TaxCategory");
				W.WriteStartElement("cac:TaxScheme");
				W.WriteStartElement("cbc:ID");
				W.WriteValue("1000");
				W.WriteEndElement();
				W.WriteStartElement("cbc:Name");
				W.WriteValue("IGV");
				W.WriteEndElement();
				W.WriteStartElement("cbc:TaxTypeCode"); //Código del Tipo de Tributo (UN/ECE 5153)
				W.WriteValue("VAT");
				W.WriteEndElement();
				W.WriteEndElement();
				W.WriteEndElement();
				W.WriteEndElement(); //TaxSubtotal                    
				W.WriteEndElement(); //TaxTotal

				#endregion

				W.WriteEndElement(); // sac:SummaryDocumentsLine

				#endregion  </sac:SummaryDocumentsLine>
			}
			catch (Exception)
			{
				throw;
			}
		}

		#endregion



		public bool GenerarCodigoQR(Transaccion oTransaccion, Empresa oEmpresa)
		{
			try
			{
				string sRutaImagengQR = string.Empty;

				string strNombreArchivo = string.Empty;
				strNombreArchivo = GenerarNombreXML(oTransaccion, oEmpresa);

				#region RUTA

				switch (oTransaccion.TipoDocumento_Id)
				{
					case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_FACTURA:

						sRutaImagengQR = @""+oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_FACTURAS + strNombreArchivo + @"\" + strNombreArchivo + ".bmp";
						break;

					case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_BOLETA:
						sRutaImagengQR = @"" + oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_BOLETAS + strNombreArchivo + @"\" + strNombreArchivo + ".bmp";
						break;

					case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_NOTA_CREDITO:
						sRutaImagengQR = @"" + oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_NOTA_CREDITO + strNombreArchivo + @"\" + strNombreArchivo + ".bmp";
						break;

					case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_NOTA_DEBITO:
						sRutaImagengQR = @"" + oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_NOTA_DEBITO + strNombreArchivo + @"\" + strNombreArchivo + ".bmp";
						break;

					case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_GUIA_REMISION:
						sRutaImagengQR = @"" + oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_GUIA_REMISION + strNombreArchivo + @"\" + strNombreArchivo + ".bmp";
						break;

				}

				#endregion

				LogApplicationNeg.Instance.GuardarLogAplicacion(null, "Entrando a crear el directorio QR");
				CrearDirectorioQR(oTransaccion.TipoDocumento_Id, strNombreArchivo, oEmpresa);
				LogApplicationNeg.Instance.GuardarLogAplicacion(null, "Directorio QR Creado");

				//codigo QR-> RUC | TIPO DE DOCUMENTO | SERIE | NUMERO | MTO TOTAL IGV |MTO TOTAL DEL COMPROBANTE | FECHA DE EMISION | TIPO DE DOCUMENTO ADQUIRENTE | NUMERO DE DOCUMENTO ADQUIRENTE|

				#region Construccion del Codigo QR en Cadena

				string sQr = string.Empty;

				//sQr = oTransaccion.sTraRUCEmpresa + "|";

				//switch (oTransaccion.TipoDocumento_Id)
				//{
				//	#region 02 - Factura - > sunat 01

				//	case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_FACTURA:

				//		sQr += "01" + "|";

				//		break;

				//	#endregion

				//	#region 01 - Boleta - > sunat 03

				//	case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_BOLETA:

				//		sQr += "03" + "|";

				//		break;

				//	#endregion

				//	#region 19 - Nota de Crédito - > sunat 07

				//	case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_NOTA_CREDITO:

				//		sQr += "07" + "|";

				//		break;

				//	#endregion

				//	#region 49 - Nota de Debito - > sunat 08

				//	case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_NOTA_DEBITO:

				//		sQr += "08" + "|";

				//		break;

				//	#endregion

				//	#region 49 - Nota de Debito - > sunat 08

				//	case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_GUIA_REMISION:

				//		sQr += "09" + "|";

				//		break;

				//		#endregion

				//}

				//sQr += oTransaccion.sTraSerie + "|" + oTransaccion.sTraNumero + "|";
				//sQr += "" + oTransaccion.nTraMontoIGV + "|";
				//sQr += "" + oTransaccion.nTraImporteTotal + "|";
				//sQr += "" + oTransaccion.dTraFecha.ToString("yyyy-MM-dd") + "|";


				//if (oTransaccion.sCliNroIdentidad.Length == 8)
				//{
				//	sQr += "1" + "|";
				//}
				//else
				//{
				//	sQr += "6" + "|";
				//}

				//sQr += "" + oTransaccion.sCliNroIdentidad + "|";
				sQr = oTransaccion.sTraTextoQR;

				#endregion

				QRCodeEncoder encoder = new QRCodeEncoder();
				Bitmap img = encoder.Encode(sQr);
				Image QR = (Image)img;

				#region Construir y Guardar hash

				var encoding = new UTF8Encoding();
				var bm = new Bitmap(QR);
				int finalW = (bm.Width * 1);
				int finalH = (bm.Height * 1);
				var retBimap = new Bitmap(finalW, finalH);
				var retgr = Graphics.FromImage(retBimap);
				retgr.ScaleTransform(1, 1);
				retgr.SmoothingMode = SmoothingMode.HighQuality;
				retgr.InterpolationMode = InterpolationMode.NearestNeighbor;
				retgr.PixelOffsetMode = PixelOffsetMode.HighQuality;
				retgr.DrawImage(bm, new Point(0, 0));

				if (!File.Exists(sRutaImagengQR))
					retBimap.Save(sRutaImagengQR, ImageFormat.Bmp);

				GC.Collect();

				#endregion
				LogApplicationNeg.Instance.GuardarLogAplicacion(null, "Archivo insertado dentro del directorio QR");
				return true;
			}
			catch (Exception ex)
			{
				LogApplicationNeg.Instance.GuardarLogAplicacion(ex);
				throw;
			}
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="oTransaccion"></param>
		/// <param name="oEmpresa"></param>
		/// <returns></returns>
		public bool GenerarCodigoHash(Transaccion oTransaccion, Empresa oEmpresa)
		{
			try
			{
				string Xml = string.Empty;
				string strNombreArchivo = string.Empty;

				strNombreArchivo = GenerarNombreXML(oTransaccion, oEmpresa);

				#region RUTA

				switch (oTransaccion.TipoDocumento_Id)
				{
					case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_FACTURA:

						Xml = oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_FACTURAS + strNombreArchivo + @"\" + strNombreArchivo + ".xml";
						break;

					case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_BOLETA:
						Xml = oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_BOLETAS + strNombreArchivo + @"\" + strNombreArchivo + ".xml";
						break;

					case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_NOTA_CREDITO:
						Xml = oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_NOTA_CREDITO + strNombreArchivo + @"\" + strNombreArchivo + ".xml";
						break;

					case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_NOTA_DEBITO:
						Xml = oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_NOTA_DEBITO + strNombreArchivo + @"\" + strNombreArchivo + ".xml";
						break;
				}

				#endregion

				#region DECLARE XML

				var xmlEnvio = new XmlDocument();
				xmlEnvio.Load(Xml);
				xmlEnvio.PreserveWhitespace = true;
				var _mngCR = new XmlNamespaceManager(xdocCR.NameTable);

				#endregion

				#region ESPECIFICACION

				switch (oTransaccion.TipoDocumento_Id)
				{
					case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_FACTURA:

						_mngCR.AddNamespace("tns", "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2");
						break;

					case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_BOLETA:
						_mngCR.AddNamespace("tns", "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2");
						break;

					case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_NOTA_CREDITO:
						_mngCR.AddNamespace("tns", "urn:oasis:names:specification:ubl:schema:xsd:CreditNote-2");

						break;

					case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_NOTA_DEBITO:
						_mngCR.AddNamespace("tns", "urn:oasis:names:specification:ubl:schema:xsd:DebitNote-2");
						break;
				}

				#endregion

				#region COMPONENTES

				_mngCR.AddNamespace("sac", "urn:sunat:names:specification:ubl:peru:schema:xsd:SunatAggregateComponents-1");
				_mngCR.AddNamespace("cac", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
				_mngCR.AddNamespace("cbc", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
				_mngCR.AddNamespace("udt", "urn:un:unece:uncefact:data:specification:UnqualifiedDataTypesSchemaModule:2");
				_mngCR.AddNamespace("ccts", "urn:un:unece:uncefact:documentation:2");
				_mngCR.AddNamespace("ext", "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2");
				_mngCR.AddNamespace("qdt", "urn:oasis:names:specification:ubl:schema:xsd:QualifiedDatatypes-2");
				_mngCR.AddNamespace("ds", "http://www.w3.org/2000/09/xmldsig#");

				#endregion

				#region schemaLocation

				switch (oTransaccion.TipoDocumento_Id)
				{
					case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_FACTURA:

						_mngCR.AddNamespace("schemaLocation",
					"urn:oasis:names:specification:ubl:schema:xsd:Invoice-2 ..\\xsd\\maindoc\\UBLPE-Invoice-2.0.xsd");
						break;

					case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_BOLETA:
						_mngCR.AddNamespace("schemaLocation",
					"urn:oasis:names:specification:ubl:schema:xsd:Invoice-2 ..\\xsd\\maindoc\\UBLPE-Invoice-2.0.xsd");
						break;

					case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_NOTA_CREDITO:
						_mngCR.AddNamespace("schemaLocation",
					"urn:oasis:names:specification:ubl:schema:xsd:Invoice-2 ..\\xsd\\maindoc\\UBLPE-CreditNote-2.0.xsd");

						break;

					case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_NOTA_DEBITO:
						_mngCR.AddNamespace("schemaLocation",
					"urn:oasis:names:specification:ubl:schema:xsd:Invoice-2 ..\\xsd\\maindoc\\UBLPE-DebitNote-2.0.xsd");
						break;
				}

				_mngCR.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");

				#endregion

				#region INICIANDO VALORES PARA HASH

				XmlNode nodeRUC = null;
				XmlNode nodeTipoDocumento = null;
				XmlNode nodeSerieNumero = null;
				XmlNode nodeTotalIGV = null;
				XmlNode nodeTotalComprobante = null;
				XmlNode nodeFechaEmision = null;
				XmlNode nodeTipoDocAdquiriente = null;
				XmlNode nodeNumDocAdquiriente = null;
				XmlNode nodeFirma = null;
				XmlNode nodeSignedInfo = null;
				XmlNode nodeResumenFirma = null;
				XmlNode nodeValorFirma = null;

				#endregion

				#region CUERPO

				switch (oTransaccion.TipoDocumento_Id)
				{
					case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_FACTURA:

						nodeRUC =
					 xmlEnvio.SelectSingleNode("/tns:Invoice/cac:AccountingSupplierParty/cbc:CustomerAssignedAccountID",
						 _mngCR);
						nodeTipoDocumento = xmlEnvio.SelectSingleNode("/tns:Invoice/cbc:InvoiceTypeCode", _mngCR);
						nodeSerieNumero = xmlEnvio.SelectSingleNode("/tns:Invoice/cbc:ID", _mngCR);
						nodeTotalIGV = xmlEnvio.SelectSingleNode("/tns:Invoice/cac:TaxTotal/cbc:TaxAmount", _mngCR);
						nodeTotalComprobante = xmlEnvio.SelectSingleNode("/tns:Invoice/cac:LegalMonetaryTotal", _mngCR);
						nodeFechaEmision = xmlEnvio.SelectSingleNode("/tns:Invoice/cbc:IssueDate", _mngCR);
						nodeTipoDocAdquiriente =
							xmlEnvio.SelectSingleNode("/tns:Invoice/cac:AccountingCustomerParty/cbc:AdditionalAccountID", _mngCR);
						nodeNumDocAdquiriente =
							xmlEnvio.SelectSingleNode("/tns:Invoice/cac:AccountingCustomerParty/cbc:CustomerAssignedAccountID",
								_mngCR);
						nodeFirma =
							xmlEnvio.SelectSingleNode(
								"/tns:Invoice/ext:UBLExtensions/ext:UBLExtension[1]/ext:ExtensionContent/ds:Signature", _mngCR);
						nodeSignedInfo = nodeFirma.FirstChild.LastChild.LastChild;
						nodeResumenFirma =
							xmlEnvio.SelectSingleNode(
								"/tns:Invoice/ext:UBLExtensions/ext:UBLExtension[1]/ext:ExtensionContent/ds:Signature", _mngCR);
						nodeValorFirma = nodeResumenFirma.FirstChild.NextSibling;

						break;

					case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_BOLETA:
						nodeRUC =
					xmlEnvio.SelectSingleNode("/tns:Invoice/cac:AccountingSupplierParty/cbc:CustomerAssignedAccountID",
						_mngCR);
						nodeTipoDocumento = xmlEnvio.SelectSingleNode("/tns:Invoice/cbc:InvoiceTypeCode", _mngCR);
						nodeSerieNumero = xmlEnvio.SelectSingleNode("/tns:Invoice/cbc:ID", _mngCR);
						nodeTotalIGV = xmlEnvio.SelectSingleNode("/tns:Invoice/cac:TaxTotal/cbc:TaxAmount", _mngCR);
						nodeTotalComprobante = xmlEnvio.SelectSingleNode("/tns:Invoice/cac:LegalMonetaryTotal", _mngCR);
						nodeFechaEmision = xmlEnvio.SelectSingleNode("/tns:Invoice/cbc:IssueDate", _mngCR);
						nodeTipoDocAdquiriente =
							xmlEnvio.SelectSingleNode("/tns:Invoice/cac:AccountingCustomerParty/cbc:AdditionalAccountID", _mngCR);
						nodeNumDocAdquiriente =
							xmlEnvio.SelectSingleNode("/tns:Invoice/cac:AccountingCustomerParty/cbc:CustomerAssignedAccountID",
								_mngCR);
						nodeFirma =
							xmlEnvio.SelectSingleNode(
								"/tns:Invoice/ext:UBLExtensions/ext:UBLExtension[1]/ext:ExtensionContent/ds:Signature", _mngCR);
						nodeSignedInfo = nodeFirma.FirstChild.LastChild.LastChild;
						nodeResumenFirma =
							xmlEnvio.SelectSingleNode(
								"/tns:Invoice/ext:UBLExtensions/ext:UBLExtension[1]/ext:ExtensionContent/ds:Signature", _mngCR);
						nodeValorFirma = nodeResumenFirma.FirstChild.NextSibling;
						break;

					case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_NOTA_CREDITO:
						nodeRUC =
				   xmlEnvio.SelectSingleNode(
					   "/tns:CreditNote/cac:AccountingSupplierParty/cbc:CustomerAssignedAccountID", _mngCR);
						//nodeTipoDocumento.InnerText = "07";
						nodeSerieNumero = xmlEnvio.SelectSingleNode("/tns:CreditNote/cbc:ID", _mngCR);
						nodeTotalIGV = xmlEnvio.SelectSingleNode("/tns:CreditNote/cac:TaxTotal/cbc:TaxAmount", _mngCR);
						nodeTotalComprobante = xmlEnvio.SelectSingleNode("/tns:CreditNote/cac:LegalMonetaryTotal", _mngCR);
						nodeFechaEmision = xmlEnvio.SelectSingleNode("/tns:CreditNote/cbc:IssueDate", _mngCR);
						nodeTipoDocAdquiriente =
							xmlEnvio.SelectSingleNode("/tns:CreditNote/cac:AccountingCustomerParty/cbc:AdditionalAccountID",
								_mngCR);
						nodeNumDocAdquiriente =
							xmlEnvio.SelectSingleNode(
								"/tns:CreditNote/cac:AccountingCustomerParty/cbc:CustomerAssignedAccountID", _mngCR);
						nodeFirma =
							xmlEnvio.SelectSingleNode(
								"/tns:CreditNote/ext:UBLExtensions/ext:UBLExtension[1]/ext:ExtensionContent/ds:Signature",
								_mngCR);
						nodeSignedInfo = nodeFirma.FirstChild.LastChild.LastChild;
						nodeResumenFirma =
							xmlEnvio.SelectSingleNode(
								"/tns:CreditNote/ext:UBLExtensions/ext:UBLExtension[1]/ext:ExtensionContent/ds:Signature",
								_mngCR);
						nodeValorFirma = nodeResumenFirma.FirstChild.NextSibling;

						break;

					case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_NOTA_DEBITO:
						nodeRUC =
				   xmlEnvio.SelectSingleNode(
					   "/tns:DebitNote/cac:AccountingSupplierParty/cbc:CustomerAssignedAccountID", _mngCR);
						//nodeTipoDocumento.InnerText = "08";
						nodeSerieNumero = xmlEnvio.SelectSingleNode("/tns:DebitNote/cbc:ID", _mngCR);
						nodeTotalIGV = xmlEnvio.SelectSingleNode("/tns:DebitNote/cac:TaxTotal/cbc:TaxAmount", _mngCR);
						nodeTotalComprobante = xmlEnvio.SelectSingleNode("/tns:DebitNote/cac:RequestedMonetaryTotal", _mngCR);
						nodeFechaEmision = xmlEnvio.SelectSingleNode("/tns:DebitNote/cbc:IssueDate", _mngCR);
						nodeTipoDocAdquiriente =
							xmlEnvio.SelectSingleNode("/tns:DebitNote/cac:AccountingCustomerParty/cbc:AdditionalAccountID",
								_mngCR);
						nodeNumDocAdquiriente =
							xmlEnvio.SelectSingleNode(
								"/tns:DebitNote/cac:AccountingCustomerParty/cbc:CustomerAssignedAccountID", _mngCR);
						nodeFirma =
							xmlEnvio.SelectSingleNode(
								"/tns:DebitNote/ext:UBLExtensions/ext:UBLExtension[1]/ext:ExtensionContent/ds:Signature", _mngCR);
						nodeSignedInfo = nodeFirma.FirstChild.LastChild.LastChild;
						nodeResumenFirma =
							xmlEnvio.SelectSingleNode(
								"/tns:DebitNote/ext:UBLExtensions/ext:UBLExtension[1]/ext:ExtensionContent/ds:Signature", _mngCR);
						nodeValorFirma = nodeResumenFirma.FirstChild.NextSibling;
						break;
				}

				#endregion

				#region FORMATO HASH

				string Texto = "";

				switch (oTransaccion.TipoDocumento_Id)
				{
					case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_FACTURA:
						if (nodeRUC != null)
						{
							Texto = nodeRUC.InnerText + "|" + nodeTipoDocumento.InnerText + "|" +
							nodeSerieNumero.InnerText.Replace("-", "|") + "|" + nodeTotalIGV.InnerText + "|" +
							nodeTotalComprobante.InnerText + "|" + nodeFechaEmision.InnerText + "|";
							Texto += nodeTipoDocAdquiriente.InnerText + "|" + nodeNumDocAdquiriente.InnerText + "|" +
									 nodeSignedInfo.InnerText + "|" + nodeValorFirma.InnerText + "|";
						}
						break;

					case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_BOLETA:

						if (nodeRUC != null)
						{
							Texto = nodeRUC.InnerText + "|" + nodeTipoDocumento.InnerText + "|" +
						nodeSerieNumero.InnerText.Replace("-", "|") + "|" + nodeTotalIGV.InnerText + "|" +
						nodeTotalComprobante.InnerText + "|" + nodeFechaEmision.InnerText + "|";
							Texto += nodeTipoDocAdquiriente.InnerText + "|" + nodeNumDocAdquiriente.InnerText + "|" +
									 nodeSignedInfo.InnerText + "|" + nodeValorFirma.InnerText + "|";
						}
						break;

					case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_NOTA_CREDITO:

						if (nodeRUC != null)
						{
							Texto = nodeRUC.InnerText + "|" + "07" + "|" + nodeSerieNumero.InnerText.Replace("-", "|") + "|" +
						nodeTotalIGV.InnerText + "|" + nodeTotalComprobante.InnerText + "|" + nodeFechaEmision.InnerText +
						"|";
							Texto += nodeTipoDocAdquiriente.InnerText + "|" + nodeNumDocAdquiriente.InnerText + "|" +
									 nodeSignedInfo.InnerText + "|" + nodeValorFirma.InnerText + "|";
						}

						break;

					case (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_NOTA_DEBITO:

						if (nodeRUC != null)
						{
							Texto = nodeRUC.InnerText + "|" + "08" + "|" + nodeSerieNumero.InnerText.Replace("-", "|") + "|" +
						nodeTotalIGV.InnerText + "|" + nodeTotalComprobante.InnerText + "|" + nodeFechaEmision.InnerText +
						"|";
							Texto += nodeTipoDocAdquiriente.InnerText + "|" + nodeNumDocAdquiriente.InnerText + "|" +
									 nodeSignedInfo.InnerText + "|" + nodeValorFirma.InnerText + "|";

						}
						break;
				}

				#endregion

				#region Construir y Guardar hash

				var barcode = new BarcodePDF417();
				barcode.Options = BarcodePDF417.PDF417_FORCE_BINARY;
				barcode.CodeRows = 0;
				barcode.CodeColumns = 0;
				barcode.YHeight = 3;
				barcode.ErrorLevel = 5;
				var encoding = new UTF8Encoding();
				var b = encoding.GetBytes(Texto);
				barcode.Text = b;
				var bm = new Bitmap(barcode.CreateDrawingImage(Color.Black, Color.White));
				int finalW = (bm.Width * 1);
				int finalH = (bm.Height * 1);
				var retBimap = new Bitmap(finalW, finalH);
				var retgr = Graphics.FromImage(retBimap);
				retgr.ScaleTransform(1, 1);
				retgr.SmoothingMode = SmoothingMode.HighQuality;
				retgr.InterpolationMode = InterpolationMode.NearestNeighbor;
				retgr.PixelOffsetMode = PixelOffsetMode.HighQuality;
				retgr.DrawImage(bm, new Point(0, 0));
				string NombreArchivo = Xml.Replace(".xml", "") + ".bmp";

				retBimap.Save(NombreArchivo, ImageFormat.Bmp);
				GC.Collect();

				#endregion

				return true;
			}
			catch (Exception ex)
			{
				LogApplicationNeg.Instance.GuardarLogAplicacion(ex);
				return false;
			}
		}

		/// <summary>
		/// Crear xml de la boleta de venta electronica con el formato de facturas
		/// </summary>
		/// <param name="oTransaccion"></param>
		/// <param name="p_oEmpresa"></param>
		public bool GenerarArchivoBoletaXml(Transaccion oTransaccion, Empresa p_oEmpresa)
		{
			try
			{
				CTA_BANCO_NACION_DETRACCIONES = p_oEmpresa.sEmpCtaBancoDetracciones.Replace("-", "");
				oEmpresa = p_oEmpresa;

				#region CREAR DOCUMENTO ELECTRONICO

				#region INICIALES

				strNombreArchivo = GenerarNombreXML(oTransaccion, oEmpresa);

				string sMoneda = string.Empty;
				//Texto para mostrar en el importe pasado a letras
				if (oTransaccion.Moneda_Id == Enumerador.MONEDA_EXTRANJERA_DOLARES_ID)
					sMoneda = @"Dolares Americanos";
				else
					sMoneda = @"Soles";

				string CODIGO_DOCUMENTO_SUNAT = string.Empty;

				//texto para mostrar CURRENCYID-> MONEDA EN PEN O USD
				string MONEDA = oTransaccion.Moneda_Id == Enumerador.MONEDA_NACIONAL_SOLES_ID ? "PEN" : "USD";

				#endregion

				#region CREAR DIRECTORIO SI NO EXISTE

				CrearDirectorio(oTransaccion.TipoDocumento_Id, strNombreArchivo, oEmpresa);

				//asegurando que la boleta xml se vuelva a generar otra vez
				Array.ForEach(Directory.GetFiles(oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_BOLETAS + strNombreArchivo),
						delegate (string path)
						{
							var test = oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_BOLETAS + strNombreArchivo + @"\" + strNombreArchivo + ".xml";
							if (path != null && test != null && path.Equals(test))
								File.Delete(path);
						});

				#endregion

				#region PREPARAR EL ESCRITOR DE XML

				XmlTextWriter W = null;
				W =
				   new XmlTextWriter(oEmpresa.sEmpRuta +
				   RUTA_REPOSITORIO_ELECTRONICO_BOLETAS + strNombreArchivo + @"\" + strNombreArchivo +
					".xml", Encoding.GetEncoding("ISO-8859-1"));

				#endregion

				W.Formatting = Formatting.Indented;
				W.WriteStartDocument(false);
				#endregion

				#region INICIO DE DOCUMENTO

				W.WriteStartElement("Invoice"); //  Documento utilizado para requerir un pago
				W.WriteStartAttribute("xmlns");
				W.WriteValue("urn:oasis:names:specification:ubl:schema:xsd:Invoice-2");

				CODIGO_DOCUMENTO_SUNAT = "03";

				#endregion

				#region NECESARIOS SEGUN MODELO SUNAT XML

				W.WriteStartAttribute("xmlns:cac");
				W.WriteValue("urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");

				W.WriteStartAttribute("xmlns:cbc");
				W.WriteValue("urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");

				W.WriteStartAttribute("xmlns:ccts");
				W.WriteValue("urn:un:unece:uncefact:documentation:2");

				W.WriteStartAttribute("xmlns:ds");
				W.WriteValue("http://www.w3.org/2000/09/xmldsig#");

				W.WriteStartAttribute("xmlns:ext");
				W.WriteValue("urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2");

				W.WriteStartAttribute("xmlns:qdt");
				W.WriteValue("urn:oasis:names:specification:ubl:schema:xsd:QualifiedDatatypes-2");

				W.WriteStartAttribute("xmlns:sac");
				W.WriteValue("urn:sunat:names:specification:ubl:peru:schema:xsd:SunatAggregateComponents-1");

				W.WriteStartAttribute("xmlns:udt");
				W.WriteValue("urn:un:unece:uncefact:data:specification:UnqualifiedDataTypesSchemaModule:2");

				W.WriteStartAttribute("xmlns:xsi");
				W.WriteValue("http://www.w3.org/2001/XMLSchema-instance");
				W.WriteEndAttribute();

				#endregion

				//CUERPO PRINCIPAL DE CONTENIDO
				/*===============================================*/
				W.WriteStartElement("ext:UBLExtensions");
				/*===============================================*/

				//EXTENSION PARA INCRUSTAR LA FIRMA DIGITAL*/
				//==================================================================
				/*  Contenedor de Componentes de extensión. 
                    Podrán incorporarse nuevas definiciones estructuradas 
                    cuando sean de interés conjunto para emisores y receptores, 
                    y no estén ya definidas en el esquema de la factura. */

				W.WriteStartElement("ext:UBLExtension");
				W.WriteStartElement("ext:ExtensionContent");
				W.WriteEndElement();
				W.WriteEndElement();
				W.WriteStartElement("ext:UBLExtension");
				W.WriteStartElement("ext:ExtensionContent");
				W.WriteStartElement("sac:AdditionalInformation"); //Información adicional recomendado por SUNAT

				#region BOLETA

				if (oTransaccion.TipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_BOLETA) //boleta
				{
					#region BOLETA VENTA GRATUITA

					if (oTransaccion.nTraEsGratuito == (int)Enumerador.ESTADO_ACTIVO) //boleta con venta gratuita
					{
						#region sac:AdditionalMonetaryTotal 1001

						W.WriteStartElement("sac:AdditionalMonetaryTotal");
						W.WriteStartElement("cbc:ID");
						W.WriteValue("1001");
						W.WriteEndElement();
						W.WriteStartElement("cbc:PayableAmount"); //Monto a pagar
						W.WriteStartAttribute("currencyID");
						W.WriteValue(MONEDA);
						W.WriteEndAttribute();
						W.WriteValue("0.00"); //Total de la Factura sin IGV (Subtotal)
						W.WriteEndElement(); //</cbc:PayableAmount>
						W.WriteEndElement(); //</sac:AdditionalMonetaryTotal>

						#endregion

						W.WriteStartElement("sac:AdditionalMonetaryTotal");
						W.WriteStartElement("cbc:ID");
						W.WriteValue("1004");
						W.WriteEndElement();
						W.WriteStartElement("cbc:PayableAmount"); //Monto a pagar
						W.WriteStartAttribute("currencyID");
						W.WriteValue(MONEDA);
						W.WriteEndAttribute();
						W.WriteValue(String.Format(CultureInfo.InvariantCulture, "{0:0.00}", oTransaccion.nTraSubTotalSinIGV));
						W.WriteEndElement();
						W.WriteEndElement(); //sac:AdditionalMonetaryTotal
						W.WriteStartElement("sac:AdditionalProperty"); //Información adicional de cualquier tipo
						W.WriteStartElement("cbc:ID"); //Código del concepto adicional
						W.WriteValue("1002");
						W.WriteEndElement();
						W.WriteStartElement("cbc:Value"); //Valor del concepto               
						W.WriteValue("TRANSFERENCIA GRATUITA");
						W.WriteEndElement();
						W.WriteEndElement(); //sac:AdditionalProperty
					}
					#endregion

					else
					{
						#region BOLETAS CON IGv GRABADO

						if (oTransaccion.LstTransaccionDetalle[0].nTDeEstadoIGV == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_IGV_GRABADO)
						{
							W.WriteStartElement("sac:AdditionalMonetaryTotal");
							W.WriteStartElement("cbc:ID"); //Código del concepto adicional
							W.WriteValue("1001");
							W.WriteEndElement();
							W.WriteStartElement("cbc:PayableAmount"); //Monto a pagar
							W.WriteStartAttribute("currencyID");
							W.WriteValue(MONEDA);
							W.WriteEndAttribute();
							W.WriteValue(String.Format(CultureInfo.InvariantCulture, "{0:0.00}", oTransaccion.nTraSubTotalSinIGV));
							W.WriteEndElement();
							W.WriteEndElement(); //sac:AdditionalMonetaryTotal
							W.WriteStartElement("sac:AdditionalMonetaryTotal");
							W.WriteStartElement("cbc:ID");
							W.WriteValue("1002");
							W.WriteEndElement();
							W.WriteStartElement("cbc:PayableAmount");
							W.WriteStartAttribute("currencyID");
							W.WriteValue(MONEDA);
							W.WriteEndAttribute();
							W.WriteValue("0.00");
							W.WriteEndElement();
							W.WriteEndElement(); //sac:AdditionalMonetaryTotal
							W.WriteStartElement("sac:AdditionalMonetaryTotal");
							W.WriteStartElement("cbc:ID");
							W.WriteValue("1003");
							W.WriteEndElement();
							W.WriteStartElement("cbc:PayableAmount");
							W.WriteStartAttribute("currencyID");
							W.WriteValue(MONEDA);
							W.WriteEndAttribute();
							W.WriteValue("0.00");
							W.WriteEndElement();
							W.WriteEndElement(); //sac:AdditionalMonetaryTotal
							W.WriteStartElement("sac:AdditionalMonetaryTotal");
							W.WriteStartElement("cbc:ID");
							W.WriteValue("1004");
							W.WriteEndElement();
							W.WriteStartElement("cbc:PayableAmount");
							W.WriteStartAttribute("currencyID");
							W.WriteValue(MONEDA);
							W.WriteEndAttribute();
							W.WriteValue("0.00");
							W.WriteEndElement();
							W.WriteEndElement();
						}
						#endregion

						else
						{
							W.WriteStartElement("sac:AdditionalMonetaryTotal");
							W.WriteStartElement("cbc:ID");
							W.WriteValue("1003");
							W.WriteEndElement();
							W.WriteStartElement("cbc:PayableAmount"); //Monto a pagar
							W.WriteStartAttribute("currencyID");
							W.WriteValue(MONEDA);
							W.WriteEndAttribute();
							W.WriteValue(String.Format(CultureInfo.InvariantCulture, "{0:0.00}", oTransaccion.nTraSubTotalSinIGV));
							W.WriteEndElement();
							W.WriteEndElement(); //sac:AdditionalMonetaryTotal
						}

						W.WriteStartElement("sac:AdditionalProperty"); //Información adicional de cualquier tipo
						W.WriteStartElement("cbc:ID"); //Código del concepto adicional
						W.WriteValue("1000");
						W.WriteEndElement();
						W.WriteStartElement("cbc:Value"); //Valor del concepto              


						W.WriteValue(Util.ConvertirNumerosAletras(oTransaccion.nTraImporteTotal, sMoneda));
						W.WriteEndElement();
						W.WriteEndElement(); //sac:AdditionalProperty
					}
				}

				#endregion

				W.WriteEndElement(); //sac:AdditionalInformation
				W.WriteEndElement(); //ext:ExtensionContent
				W.WriteEndElement(); // ext:UBLExtension  
				W.WriteEndElement(); //</ext:UBLExtension>
									 /*--FIN DE EXTENSIONES UBL*/
				W.WriteStartElement("cbc:UBLVersionID"); //Versión del UBL
				W.WriteValue("2.1");
				W.WriteEndElement();
				W.WriteStartElement("cbc:CustomizationID"); //Versión de la estructura del documento
				W.WriteValue("2.0");
				W.WriteEndElement();
				W.WriteStartElement("cbc:ID");
				W.WriteValue(oTransaccion.sNumeroGenerado);
				W.WriteEndElement();
				W.WriteStartElement("cbc:IssueDate");
				W.WriteValue(oTransaccion.dTraFecha.ToString("yyyy-MM-dd"));
				W.WriteEndElement();

				#region FACTURA O BOLETA

				if (
					oTransaccion.TipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_FACTURA ||
					oTransaccion.TipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_BOLETA
					)
				{
					W.WriteStartElement("cbc:InvoiceTypeCode");
					W.WriteValue(CODIGO_DOCUMENTO_SUNAT);
					W.WriteEndElement();
				}

				#endregion

				W.WriteStartElement("cbc:DocumentCurrencyCode");
				W.WriteValue(MONEDA);
				W.WriteEndElement();

				#region NOTA DE CREDITO O NOTA DE DEBITO

				if (
					oTransaccion.TipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_NOTA_CREDITO ||
					oTransaccion.TipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_NOTA_DEBITO
					)
				{
					// ***** Motivo por el cual se emite la nota de Credito
					W.WriteStartElement("cac:DiscrepancyResponse");
					W.WriteStartElement("cbc:ReferenceID"); //Identifica el numero que se aplica la nota de Credito
					W.WriteValue(oTransaccion.sSerieNumeroRelacionado);
					W.WriteEndElement();

					#region NOTA DE DEBITO

					if (oTransaccion.TipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_NOTA_DEBITO)
					{
						W.WriteStartElement("cbc:ResponseCode"); //Código por el cual se emite la nota de debito
						W.WriteValue("01"); //Código 01 intereses por mora,02 Aumento en el valor,03 Penalidades
						W.WriteEndElement();
						W.WriteStartElement("cbc:Description"); //Sustento
						W.WriteValue("nota de debito"); /*OJO POR INVESTIGAR QUE PRODUCTO TENDRA QUE IR*/
						W.WriteEndElement();
					}
					#endregion

					else
					{
						#region NOTA DE CREDITO

						W.WriteStartElement("cbc:ResponseCode"); //Código por el cual se emite la nota de credito
						W.WriteValue(oTransaccion.nCReCodigoSunat);
						W.WriteEndElement();
						W.WriteStartElement("cbc:Description"); //Descripcion Nota Credito
						W.WriteValue(oTransaccion.sCategoriaReclamo);
						W.WriteEndElement();

						#endregion
					}

					W.WriteEndElement();
					W.WriteStartElement("cac:BillingReference"); //Referencia a un documento modificado
					W.WriteStartElement("cac:InvoiceDocumentReference"); //Asociacion al documento modificado
					W.WriteStartElement("cbc:ID"); //Documento que se modifica (serie-numero)
					W.WriteValue(oTransaccion.sSerieNumeroRelacionado);
					W.WriteEndElement();
					W.WriteStartElement("cbc:DocumentTypeCode"); //Codigo del Tipo de Documento modificado

					#region SI EL DOCUMENTO RELACIONADO DE LA NOTA DE CREDITO O DEBITO ES FACTURA

					if (oTransaccion.nTipoDocRelacionado == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_FACTURA)
					{
						W.WriteValue("01");
					}
					#endregion

					else
					{
						W.WriteValue("03");
					}

					W.WriteEndElement();
					W.WriteEndElement();
					W.WriteEndElement();
				}

				#endregion

				#region REFERENCIA A LA FIRMA DIGITAL

				/*Nota: Solo anteponer la letra "S" 
                * luego el numero completo serienumero del correlativo de la transaccion*/

				W.WriteStartElement("cac:Signature"); //Referencia a la Firma Digital
				W.WriteStartElement("cbc:ID"); //Identificador de la firma
				W.WriteValue("S" + oTransaccion.sNumeroGenerado);
				W.WriteEndElement();

				#endregion

				// ***** Datos Empresa certificado ******
				W.WriteStartElement("cac:SignatoryParty"); //Código del tipo de documento adicional (p.e. SCOP)
				W.WriteStartElement("cac:PartyIdentification"); //Parte firmante
				W.WriteStartElement("cbc:ID"); //Identificación de la parte firmante
											   //AGREGAR EL RUC DE LA EMPRESA EMISORA
				W.WriteValue(oEmpresa.sEmpRuc);
				W.WriteEndElement();
				W.WriteEndElement();
				W.WriteStartElement("cac:PartyName");
				W.WriteStartElement("cbc:Name"); //Nombre de la parte firmante
				W.WriteValue("SUNAT");
				W.WriteEndElement();
				W.WriteEndElement();
				W.WriteEndElement();
				W.WriteStartElement("cac:DigitalSignatureAttachment");
				W.WriteStartElement("cac:ExternalReference");
				W.WriteStartElement("cbc:URI");
				//Identificador de Recurso Uniforme (o URI) que identifica la localización de la firma
				/*Nota: Solo anteponer el caracter "#" 
                * luego el numero completo serienumero del correlativo de la transaccion*/
				W.WriteValue("#" + oTransaccion.sNumeroGenerado);
				W.WriteEndElement();
				W.WriteEndElement();
				W.WriteEndElement();
				W.WriteEndElement(); //cac:Signature
									 //nuevamente
									 // ***** Empresa *****
				W.WriteStartElement("cac:AccountingSupplierParty"); //Datos del Emisor del documento
				W.WriteStartElement("cbc:CustomerAssignedAccountID");
				W.WriteValue(oEmpresa.sEmpRuc);
				W.WriteEndElement();
				W.WriteStartElement("cbc:AdditionalAccountID"); //Tipo de documento de identificación

				/*Nota: 
                 * RECEPTOR CLIENTE SE PONEN LO MISMO.
                 Por lo tanto se sigue el mismo standar*/

				W.WriteValue(oEmpresa.sEmpTipoIdentificadorEmpresaSunat);
				W.WriteEndElement();

				#region NOMBRE DE LA EMPRESA EMISORA

				W.WriteStartElement("cac:Party");
				W.WriteStartElement("cac:PartyName");
				W.WriteStartElement("cbc:Name"); //Nombre Comercial
				W.WriteValue(oEmpresa.sEmpNombre);
				W.WriteEndElement();
				W.WriteEndElement();

				#endregion

				#region UBIGEO

				W.WriteStartElement("cac:PostalAddress"); //Domicilio fiscal
				W.WriteStartElement("cbc:ID"); //Código de UBIGEO
				W.WriteValue(oEmpresa.sEmpCodigoUbigeoSunat);
				W.WriteEndElement();

				#endregion

				#region DIRECCION

				W.WriteStartElement("cbc:StreetName");
				W.WriteValue(oEmpresa.sEmpDireccion);
				W.WriteEndElement();

				#endregion

				#region DEPARTAMENTO

				W.WriteStartElement("cbc:CityName"); // Departamento
				W.WriteValue(oEmpresa.sEmpCiudad);
				W.WriteEndElement();

				#endregion

				W.WriteStartElement("cac:Country"); // Código del País
				W.WriteStartElement("cbc:IdentificationCode"); // Código del País
				W.WriteValue(oEmpresa.sEmpAbreviaturaPais);
				W.WriteEndElement();
				W.WriteEndElement();
				W.WriteEndElement(); //cac:PostalAddress
				W.WriteStartElement("cac:PartyLegalEntity");
				W.WriteStartElement("cbc:RegistrationName"); // Apellidos y nombres o denominación o razón social
				W.WriteValue(oEmpresa.sEmpNombre);
				W.WriteEndElement();
				W.WriteEndElement();
				W.WriteEndElement(); //cac:Party         
				W.WriteEndElement(); //cac:AccountingSupplierParty

				#region BOLETA

				if (oTransaccion.TipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_BOLETA)
				{
					W.WriteStartElement("cac:AccountingCustomerParty"); //Datos del Adquirente o Usuario
					W.WriteStartElement("cbc:CustomerAssignedAccountID"); //Número de documento de identidad  DNI
					W.WriteValue(oTransaccion.sCliNroIdentidad);
					W.WriteEndElement();
					W.WriteStartElement("cbc:AdditionalAccountID"); //Tipo de documento de identificación
																	//SEGUN LOS CODIGOS DE SUNAT 1-> DNI Y 6->RUC
					if (oTransaccion.sCliNroIdentidad.Length == 8)
						W.WriteValue("1");
					else //SI ES RUC:NO SE DA PERO PODRI SER NECESARIO
						if (oTransaccion.sCliNroIdentidad.Length == 11)
						//SEGUN LOS CODIGOS DE SUNAT 1-> DNI Y 6->RUC
						W.WriteValue("6");
					W.WriteEndElement();
				}
				#endregion

				#region DIFERENTE DE BOLETA : FACTURA, ETC MOSTRAMOS RUC

				else
				{
					W.WriteStartElement("cac:AccountingCustomerParty"); //Datos del Adquirente o Usuario
					W.WriteStartElement("cbc:CustomerAssignedAccountID"); //Número de documento de identidad  RUC
					W.WriteValue(oTransaccion.sCliNroIdentidad);
					W.WriteEndElement();
					W.WriteStartElement("cbc:AdditionalAccountID"); //Tipo de documento de identificación
					W.WriteValue("6");
					W.WriteEndElement();
				}

				#endregion

				W.WriteStartElement("cac:Party");
				W.WriteStartElement("cac:PostalAddress");
				W.WriteStartElement("cbc:ID");
				W.WriteEndElement();

				#region DIRECCION DEL CLIENTE

				W.WriteStartElement("cbc:StreetName");
				W.WriteValue(oTransaccion.sCliDomicilioFiscal);
				W.WriteEndElement();

				#endregion

				#region UBIGEO DEL CLIENTE

				W.WriteStartElement("cbc:CityName");
				W.WriteValue(oTransaccion.sCliUbigeoNombre);
				W.WriteEndElement();

				#endregion

				#region BREVE DEL PAIS DEL CLIENTE

				W.WriteStartElement("cac:Country");
				W.WriteStartElement("cbc:IdentificationCode");
				W.WriteValue(oTransaccion.sCliPaisBreve);
				W.WriteEndElement();
				W.WriteEndElement();
				W.WriteEndElement();

				#endregion

				#region NOMBRE DEL CLIENTE

				W.WriteStartElement("cac:PartyLegalEntity");
				W.WriteStartElement("cbc:RegistrationName");
				W.WriteValue(oTransaccion.sCliente);
				W.WriteEndElement();
				W.WriteEndElement();
				W.WriteEndElement();
				W.WriteEndElement(); //cac:AccountingCustomerParty

				#endregion

				// ***** IGV

				#region VALORES DE IGV CON VENTA GRATUITA

				if (
					 oTransaccion.nTraEsGratuito == Enumerador.ESTADO_ACTIVO
					 &&
					 oTransaccion.TipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_FACTURA
					)
				{
					W.WriteStartElement("cac:TaxTotal"); //Impuestos Globales
					W.WriteStartElement("cbc:TaxAmount"); //Importe total de un tributo para la factura
					W.WriteStartAttribute("currencyID");
					W.WriteValue(MONEDA);
					W.WriteEndAttribute();
					W.WriteValue("0.00");
					W.WriteEndElement(); //cbc:TaxAmount
					W.WriteStartElement("cac:TaxSubtotal");
					W.WriteStartElement("cbc:TaxAmount"); //Importe total de un tributo para la factura
					W.WriteStartAttribute("currencyID"); //Importe explícito a tributar
					W.WriteValue(MONEDA);
					W.WriteEndAttribute();
					W.WriteValue("0.00");
					W.WriteEndElement(); //cbc:TaxAmount  
					W.WriteStartElement("cac:TaxCategory");
					W.WriteStartElement("cac:TaxScheme");
					W.WriteStartElement("cbc:ID"); //Identificación del tributo según Catálogo No. 05
					W.WriteValue("1000");
					W.WriteEndElement();
					W.WriteStartElement("cbc:Name"); //Nombre del Tributo (IGV, ISC)
					W.WriteValue("IGV");
					W.WriteEndElement();
					W.WriteStartElement("cbc:TaxTypeCode"); //Código del Tipo de Tributo (UN/ECE 5153)
					W.WriteValue("VAT");
					W.WriteEndElement();
					W.WriteEndElement();
					W.WriteEndElement(); //cac:TaxCategory
					W.WriteEndElement(); //cac:TaxSubtotal
					W.WriteEndElement(); //cac:TaxTotal
				}

				#endregion

				else
				#region IGV DE BOLETA CON VENTA GRATUITA

					if (
						 oTransaccion.nTraEsGratuito == Enumerador.ESTADO_ACTIVO
						 &&
						 oTransaccion.TipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_BOLETA
						)
				{
					W.WriteStartElement("cac:TaxTotal"); //Impuestos Globales
					W.WriteStartElement("cbc:TaxAmount"); //Importe total de un tributo para la factura
					W.WriteStartAttribute("currencyID");
					W.WriteValue(MONEDA);
					W.WriteEndAttribute();
					W.WriteValue("0.00");
					W.WriteEndElement(); //cbc:TaxAmount
					W.WriteStartElement("cac:TaxSubtotal");
					W.WriteStartElement("cbc:TaxAmount"); //Importe total de un tributo para la factura
					W.WriteStartAttribute("currencyID"); //Importe explícito a tributar
					W.WriteValue(MONEDA);
					W.WriteEndAttribute();
					W.WriteValue("0.00");
					W.WriteEndElement(); //cbc:TaxAmount  
					W.WriteStartElement("cac:TaxCategory");
					W.WriteStartElement("cac:TaxScheme");
					W.WriteStartElement("cbc:ID"); //Identificación del tributo según Catálogo No. 05
					W.WriteValue("9996");
					W.WriteEndElement();
					W.WriteStartElement("cbc:Name"); //Nombre del Tributo (IGV, ISC)
					W.WriteValue("GRA");
					W.WriteEndElement();
					W.WriteStartElement("cbc:TaxTypeCode"); //Código del Tipo de Tributo (UN/ECE 5153)
					W.WriteValue("VAT");
					W.WriteEndElement();
					W.WriteEndElement();
					W.WriteEndElement(); //cac:TaxCategory
					W.WriteEndElement(); //cac:TaxSubtotal
					W.WriteEndElement(); //cac:TaxTotal
				}
				#endregion
				else
				{
					#region ANTICIPO

					if (oTransaccion.nTraEsAnticipo == Enumerador.ESTADO_ACTIVO)
					{
						W.WriteStartElement("cac:PrepaidPayment");
						W.WriteStartElement("cbc:ID");
						W.WriteStartAttribute("schemeID");

						if (oTransaccion.TipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_FACTURA)
						{
							W.WriteValue("02");
						}
						else if (oTransaccion.TipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_BOLETA)
						{
							W.WriteValue("03");
						}

						W.WriteEndAttribute();
						W.WriteValue(oTransaccion.sNumeroGenerado);
						W.WriteEndElement();
						W.WriteStartElement("cbc:PaidAmount");
						W.WriteStartAttribute("currencyID");
						W.WriteValue(MONEDA);
						W.WriteEndAttribute();
						W.WriteValue(oTransaccion.nTraSubTotalSinIGV.ToString("N2"));
						W.WriteEndElement();
						W.WriteStartElement("cbc:InstructionID");
						W.WriteStartAttribute("schemeID");
						W.WriteValue(oEmpresa.sEmpTipoIdentificadorEmpresaSunat);
						W.WriteEndAttribute();
						W.WriteValue(oEmpresa.sEmpRuc);
						W.WriteEndElement();
						W.WriteEndElement();
						W.WriteStartElement("cac:TaxTotal"); //Impuestos Globales
						W.WriteStartElement("cbc:TaxAmount"); //Importe total de un tributo para la factura
						W.WriteStartAttribute("currencyID");
						W.WriteValue(MONEDA);
						W.WriteEndAttribute();
						W.WriteValue(String.Format(CultureInfo.InvariantCulture, "{0:0.00}", oTransaccion.nTraMontoIGV));
						W.WriteEndElement(); //cbc:TaxAmount
						W.WriteStartElement("cac:TaxSubtotal");
						W.WriteStartElement("cbc:TaxAmount"); //Importe total de un tributo para la factura
						W.WriteStartAttribute("currencyID"); //Importe explícito a tributar
						W.WriteValue(MONEDA);
						W.WriteEndAttribute();
						W.WriteValue(String.Format(CultureInfo.InvariantCulture, "{0:0.00}", oTransaccion.nTraMontoIGV));
						W.WriteEndElement(); //cbc:TaxAmount  
						W.WriteStartElement("cac:TaxCategory");
						W.WriteStartElement("cac:TaxScheme");
						W.WriteStartElement("cbc:ID"); //Identificación del tributo según Catálogo No. 05
						W.WriteValue("1000");
						W.WriteEndElement();
						W.WriteStartElement("cbc:Name"); //Nombre del Tributo (IGV, ISC)
						W.WriteValue("IGV");
						W.WriteEndElement();
						W.WriteStartElement("cbc:TaxTypeCode"); //Código del Tipo de Tributo (UN/ECE 5153)
						W.WriteValue("VAT");
						W.WriteEndElement();
						W.WriteEndElement();
						W.WriteEndElement(); //cac:TaxCategory
						W.WriteEndElement(); //cac:TaxSubtotal
						W.WriteEndElement(); //cac:TaxTotal
					}
					#endregion
					else
					{
						W.WriteStartElement("cac:TaxTotal"); //Impuestos Globales
						W.WriteStartElement("cbc:TaxAmount"); //Importe total de un tributo para la factura
						W.WriteStartAttribute("currencyID");
						W.WriteValue(MONEDA);
						W.WriteEndAttribute();
						W.WriteValue(String.Format(CultureInfo.InvariantCulture, "{0:0.00}", oTransaccion.nTraMontoIGV));
						W.WriteEndElement(); //cbc:TaxAmount
						W.WriteStartElement("cac:TaxSubtotal");
						W.WriteStartElement("cbc:TaxAmount"); //Importe total de un tributo para la factura
						W.WriteStartAttribute("currencyID"); //Importe explícito a tributar
						W.WriteValue(MONEDA);
						W.WriteEndAttribute();
						W.WriteValue(String.Format(CultureInfo.InvariantCulture, "{0:0.00}", oTransaccion.nTraMontoIGV));
						W.WriteEndElement(); //cbc:TaxAmount  
						W.WriteStartElement("cac:TaxCategory");
						W.WriteStartElement("cac:TaxScheme");
						W.WriteStartElement("cbc:ID"); //Identificación del tributo según Catálogo No. 05
						W.WriteValue("1000");
						W.WriteEndElement();
						W.WriteStartElement("cbc:Name"); //Nombre del Tributo (IGV, ISC)
						W.WriteValue("IGV");
						W.WriteEndElement();
						W.WriteStartElement("cbc:TaxTypeCode"); //Código del Tipo de Tributo (UN/ECE 5153)
						W.WriteValue("VAT");
						W.WriteEndElement();
						W.WriteEndElement();
						W.WriteEndElement(); //cac:TaxCategory
						W.WriteEndElement(); //cac:TaxSubtotal
						W.WriteEndElement(); //cac:TaxTotal
					}
				}

				#region NOTA DE DEBITO

				if (oTransaccion.TipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_NOTA_DEBITO) //Nota de Debito
				{
					W.WriteStartElement("cac:RequestedMonetaryTotal"); //Totales a pagar y Cargos
					W.WriteStartElement("cbc:PayableAmount"); //Moneda e Importe total a pagar
					W.WriteStartAttribute("currencyID");
					W.WriteValue(MONEDA);
					W.WriteEndAttribute();
					/*TOTAL DE LA SUMA DE BASE + IGV*/
					W.WriteValue(String.Format(CultureInfo.InvariantCulture, "{0:0.00}", oTransaccion.nTraImporteTotal));
					W.WriteEndElement(); //cbc:PayableAmount
					W.WriteEndElement(); //cac:LegalMonetaryTotal
				}

				#endregion

				else
				{
					//factura con venta gratuita
					if (oTransaccion.nTraEsGratuito == Enumerador.ESTADO_ACTIVO
						&& oTransaccion.TipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_FACTURA)
					{
						W.WriteStartElement("cac:LegalMonetaryTotal"); //Totales a pagar de la Factura y Cargos
						W.WriteStartElement("cbc:PayableAmount");
						W.WriteStartAttribute("currencyID");
						W.WriteValue(MONEDA);
						W.WriteEndAttribute();
						W.WriteValue("0.00");
						W.WriteEndElement(); //cbc:PayableAmount
						W.WriteEndElement(); //cac:LegalMonetaryTotal
					}
					else if (oTransaccion.nTraEsGratuito == Enumerador.ESTADO_ACTIVO
							 && oTransaccion.TipoDocumento_Id == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_BOLETA)
					{
						W.WriteStartElement("cac:LegalMonetaryTotal"); //Totales a pagar de la Factura y Cargos
						W.WriteStartElement("cbc:PayableAmount");
						W.WriteStartAttribute("currencyID");
						W.WriteValue(MONEDA);
						W.WriteEndAttribute();
						W.WriteValue("0.00");
						W.WriteEndElement(); //cbc:PayableAmount
						W.WriteEndElement(); //cac:LegalMonetaryTotal
					}
					else
					{
						if (oTransaccion.nTraSubTotalSinIGV == 0 && oTransaccion.nTraEsAnticipo == Enumerador.ESTADO_ACTIVO)
						{
							W.WriteStartElement("cac:LegalMonetaryTotal"); //Totales a pagar de la Factura y Cargos
							W.WriteStartElement("cbc:PrepaidAmount");
							W.WriteStartAttribute("currencyID");
							W.WriteValue(MONEDA);
							W.WriteEndAttribute();
							W.WriteValue(String.Format(CultureInfo.InvariantCulture, "{0:0.00}", oTransaccion.nTraImporteTotal));
							W.WriteEndElement(); //cbc:PayableAmount
							W.WriteStartElement("cbc:PayableAmount");
							W.WriteStartAttribute("currencyID");
							W.WriteValue(MONEDA);
							W.WriteEndAttribute();
							W.WriteValue(String.Format(CultureInfo.InvariantCulture, "{0:0.00}", oTransaccion.nTraMontoIGV));
							W.WriteEndElement(); //cbc:PayableAmount
							W.WriteEndElement(); //cac:LegalMonetaryTotal
						}

						else if (oTransaccion.nTraSubTotalSinIGV > 0 &&
								 oTransaccion.nTraEsAnticipo == Enumerador.ESTADO_ACTIVO)
						{
							W.WriteStartElement("cac:LegalMonetaryTotal"); //Totales a pagar de la Factura y Cargos
							W.WriteStartElement("cbc:PrepaidAmount");
							W.WriteStartAttribute("currencyID");
							W.WriteValue(MONEDA);
							W.WriteEndAttribute();
							W.WriteValue(String.Format(CultureInfo.InvariantCulture, "{0:0.00}", oTransaccion.nTraImporteTotal));
							W.WriteEndElement(); //cbc:PayableAmount
							W.WriteStartElement("cbc:PayableAmount");
							W.WriteStartAttribute("currencyID");
							W.WriteValue(MONEDA);
							W.WriteEndAttribute();
							W.WriteValue(String.Format(CultureInfo.InvariantCulture, "{0:0.00}", oTransaccion.nTraImporteTotal));
							W.WriteEndElement(); //cbc:PayableAmount
							W.WriteEndElement(); //cac:LegalMonetaryTotal
						}
						else
						{
							W.WriteStartElement("cac:LegalMonetaryTotal"); //Totales a pagar de la Factura y Cargos
							W.WriteStartElement("cbc:PayableAmount");
							W.WriteStartAttribute("currencyID");
							W.WriteValue(MONEDA);
							W.WriteEndAttribute();
							W.WriteValue(String.Format(CultureInfo.InvariantCulture, "{0:0.00}", oTransaccion.nTraImporteTotal));
							W.WriteEndElement(); //cbc:PayableAmount
							W.WriteEndElement(); //cac:LegalMonetaryTotal
						}
					}
				}

				GenerarDetalleXml(oTransaccion, W, oEmpresa);

				// ***** Copiando el XML *****
				W.WriteEndElement(); // Cerrando Nodos Principales
									 // ***** Grabando datos del XML *****
				W.Flush();
				W.Close();


				/*EN EL CASO DE LAS BOLETAS NO SE COMPRIME YA QUE NO ES NECESARIO SEGÚN EL PROCESO*/
				#region  INCRUSTAR CERTIFICADO

				Certificado(strNombreArchivo, oTransaccion.TipoDocumento_Id, p_oEmpresa, Enumerador.ESTADO_ACTIVO);

				#endregion

				return true;
			}
			catch (Exception ex)
			{
				LogApplicationNeg.Instance.GuardarLogAplicacion(ex);
				return false;
			}
		}


		/// <summary>
		/// [vnieve] Crea el archivo xml de las boletas electronicas.
		/// </summary>
		/// <param name="oTransaccion"></param>
		/// <param name="p_oEmpresa"></param>
		public bool GenerarArchivoXmlyBoletaZipUBL2_1(Transaccion oTransaccion, Empresa p_oEmpresa)
		{
			try
			{
				CTA_BANCO_NACION_DETRACCIONES = p_oEmpresa.sEmpCtaBancoDetracciones.Replace("-", "");
				oEmpresa = p_oEmpresa;

				#region INICIALES

				strNombreArchivo = GenerarNombreXML(oTransaccion, oEmpresa);

				string sMoneda = string.Empty;
				//Texto para mostrar en el importe pasado a letras
				if (oTransaccion.Moneda_Id == Enumerador.MONEDA_EXTRANJERA_DOLARES_ID)
					sMoneda = @"Dolares Americanos";
				else
					sMoneda = @"Soles";

				string CODIGO_DOCUMENTO_SUNAT = string.Empty;
				CODIGO_DOCUMENTO_SUNAT = "03";//Boletas

				//texto para mostrar CURRENCYID-> MONEDA EN PEN O USD
				string MONEDA = oTransaccion.Moneda_Id == Enumerador.MONEDA_NACIONAL_SOLES_ID ? "PEN" : "USD";

				#endregion

				#region CREAR DIRECTORIO SI NO EXISTE

				CrearDirectorio(oTransaccion.TipoDocumento_Id, strNombreArchivo, oEmpresa);

				#endregion

				#region PREPARAR EL ESCRITOR DE XML

				XmlTextWriter W = null;

				#region FACTURAS

				W = new XmlTextWriter(oEmpresa.sEmpRuta + RUTA_REPOSITORIO_ELECTRONICO_BOLETAS + strNombreArchivo + @"\" + strNombreArchivo +
										".xml", Encoding.GetEncoding("ISO-8859-1"));

				#endregion

				W.Formatting = Formatting.Indented;
				W.WriteStartDocument(false);
				#endregion

				#region INICIO DE DOCUMENTO

				W.WriteStartElement("Invoice"); //  Documento utilizado para requerir un pago
				W.WriteStartAttribute("xmlns");
				W.WriteValue("urn:oasis:names:specification:ubl:schema:xsd:Invoice-2");

				#endregion

				#region NECESARIOS SEGUN MODELO SUNAT XML

				W.WriteStartAttribute("xmlns:cac");
				W.WriteValue("urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");

				W.WriteStartAttribute("xmlns:cbc");
				W.WriteValue("urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");

				W.WriteStartAttribute("xmlns:ccts");
				W.WriteValue("urn:un:unece:uncefact:documentation:2");

				W.WriteStartAttribute("xmlns:ds");
				W.WriteValue("http://www.w3.org/2000/09/xmldsig#");

				W.WriteStartAttribute("xmlns:ext");
				W.WriteValue("urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2");

				W.WriteStartAttribute("xmlns:qdt");
				W.WriteValue("urn:oasis:names:specification:ubl:schema:xsd:QualifiedDatatypes-2");

				W.WriteStartAttribute("xmlns:udt");
				W.WriteValue("urn:un:unece:uncefact:data:specification:UnqualifiedDataTypesSchemaModule:2");

				W.WriteStartAttribute("xmlns:xsi");
				W.WriteValue("http://www.w3.org/2001/XMLSchema-instance");

				W.WriteEndAttribute();

				#endregion

				//CUERPO PRINCIPAL DE CONTENIDO
				/*===============================================*/
				W.WriteStartElement("ext:UBLExtensions");
				/*===============================================*/

				//EXTENSION PARA INCRUSTAR LA FIRMA DIGITAL*/
				//==================================================================
				/*  Contenedor de Componentes de extensión. 
                    Podrán incorporarse nuevas definiciones estructuradas 
                    cuando sean de interés conjunto para emisores y receptores, 
                    y no estén ya definidas en el esquema de la factura. */

				W.WriteStartElement("ext:UBLExtension");
				W.WriteStartElement("ext:ExtensionContent");
				W.WriteEndElement();
				W.WriteEndElement();//ext:UBLExtension

				W.WriteEndElement(); //</ext:UBLExtension>
									 /*=================================================================*/
									 /*--FIN DE EXTENSIONES UBL*/

				/*--FIN DE EXTENSIONES UBL*/
				W.WriteStartElement("cbc:UBLVersionID"); //Versión del UBL
				W.WriteValue("2.1");
				W.WriteEndElement();
				W.WriteStartElement("cbc:CustomizationID"); //Versión de la estructura del documento
				W.WriteValue("2.0");
				W.WriteEndElement();
				W.WriteStartElement("cbc:ID");
				W.WriteValue(oTransaccion.sNumeroGenerado);
				W.WriteEndElement();
				W.WriteStartElement("cbc:IssueDate");
				W.WriteValue(oTransaccion.dTraFecha.ToString("yyyy-MM-dd"));
				W.WriteEndElement();

				/*--------------------------------------------------------------------------*/


				W.WriteStartElement("cbc:InvoiceTypeCode");

				W.WriteStartAttribute("listAgencyName");
				W.WriteValue("PE:SUNAT");

				W.WriteStartAttribute("listName");
				W.WriteValue("SUNAT:03");

				W.WriteStartAttribute("listURI");
				W.WriteValue("urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo01");

				W.WriteStartAttribute("listID");

				if (oTransaccion.nTraTieneDetraccion == Enumerador.ESTADO_ACTIVO)
					W.WriteValue("1001");
				else
					W.WriteValue("0101");

				W.WriteStartAttribute("listSchemeURI");
				W.WriteValue("urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo51");

				W.WriteEndAttribute();
				W.WriteValue(CODIGO_DOCUMENTO_SUNAT);
				W.WriteEndElement(); //cbc:InvoiceTypeCode

				W.WriteStartElement("cbc:Note");
				W.WriteStartAttribute("languageLocaleID");
				W.WriteValue("1000");
				W.WriteEndAttribute();

				W.WriteValue(Util.ConvertirNumerosAletras(oTransaccion.nTraImporteTotal, sMoneda));
				W.WriteEndElement(); //cbc:Note


				if (oTransaccion.nTraTieneDetraccion == Enumerador.ESTADO_ACTIVO)
				{
					W.WriteStartElement("cbc:Note");
					W.WriteStartAttribute("languageLocaleID");
					W.WriteValue("2006");
					W.WriteEndAttribute();
					W.WriteValue("Operación sujeta a detracción");
					W.WriteEndElement(); //cbc:Note
				}

				if (oTransaccion.nTraEsGratuito == Enumerador.ESTADO_ACTIVO)
				{
					W.WriteStartElement("cbc:Note");
					W.WriteStartAttribute("languageLocaleID");
					W.WriteValue("1002");
					W.WriteEndAttribute();
					W.WriteValue("TRANSFERENCIA GRATUITA DE UN BIEN Y/O SERVICIO PRESTADO GRATUITAMENTE");
					W.WriteEndElement(); //cbc:Note
				}


				W.WriteStartElement("cbc:DocumentCurrencyCode");

				W.WriteStartAttribute("listID");
				W.WriteValue("ISO 4217 Alpha");
				W.WriteStartAttribute("listName");
				W.WriteValue("Currency");
				W.WriteStartAttribute("listAgencyName");
				W.WriteValue("United Nations Economic Commission for Europe");
				W.WriteEndAttribute();
				W.WriteValue(MONEDA);
				W.WriteEndElement();//DocumentCurrencyCode

				#region REFERENCIA A LA FIRMA DIGITAL

				/*Nota: Solo anteponer la letra "S" 
                * luego el numero completo serienumero del correlativo de la transaccion*/

				W.WriteStartElement("cac:Signature"); //Referencia a la Firma Digital
				W.WriteStartElement("cbc:ID"); //Identificador de la firma
				W.WriteValue("S" + oTransaccion.sNumeroGenerado);
				W.WriteEndElement();

				#endregion

				// ***** Datos Empresa certificado ******
				W.WriteStartElement("cac:SignatoryParty"); //Código del tipo de documento adicional (p.e. SCOP)

				W.WriteStartElement("cac:PartyIdentification"); //Parte firmante

				W.WriteStartElement("cbc:ID"); //Identificación de la parte firmante
											   //AGREGAR EL RUC DE LA EMPRESA EMISORA
				W.WriteValue(oEmpresa.sEmpRuc);
				W.WriteEndElement();//cbc:ID

				W.WriteEndElement();//PartyIdentification

				W.WriteStartElement("cac:PartyName");
				W.WriteStartElement("cbc:Name"); //Nombre de la parte firmante
				W.WriteValue(oEmpresa.sEmpNombre);
				W.WriteEndElement();//Name
				W.WriteEndElement();//PartyName

				W.WriteEndElement();//SignatoryParty

				W.WriteStartElement("cac:DigitalSignatureAttachment");
				W.WriteStartElement("cac:ExternalReference");
				W.WriteStartElement("cbc:URI");
				//Identificador de Recurso Uniforme (o URI) que identifica la localización de la firma
				/*Nota: Solo anteponer el caracter "#" 
                * luego el numero completo serienumero del correlativo de la transaccion*/
				W.WriteValue("#" + oTransaccion.sNumeroGenerado);
				W.WriteEndElement();
				W.WriteEndElement();
				W.WriteEndElement();
				W.WriteEndElement(); //cac:Signature
									 //nuevamente
									 // ***** Empresa *****
				W.WriteStartElement("cac:AccountingSupplierParty"); //Datos del Emisor del documento

				/*Nota: 
                 * RECEPTOR CLIENTE SE PONEN LO MISMO.
                 Por lo tanto se sigue el mismo standar*/

				#region NOMBRE DE LA EMPRESA EMISORA

				W.WriteStartElement("cac:Party");

				W.WriteStartElement("cac:PartyIdentification");
				W.WriteStartElement("cbc:ID");
				W.WriteStartAttribute("schemeID");
				W.WriteValue(oEmpresa.sEmpTipoIdentificadorEmpresaSunat);
				W.WriteEndAttribute();
				W.WriteValue(oEmpresa.sEmpRuc);
				W.WriteEndElement();//cbc:ID
				W.WriteEndElement();//cac:PartyIdentification

				W.WriteStartElement("cac:PartyName");
				W.WriteStartElement("cbc:Name"); //Nombre Comercial
				W.WriteValue(oEmpresa.sEmpNombre);
				W.WriteEndElement();//cbc:Name
				W.WriteEndElement();//PartyName

				W.WriteStartElement("cac:PartyLegalEntity");

				W.WriteStartElement("cbc:RegistrationName");
				W.WriteValue(oEmpresa.sEmpNombre);
				W.WriteEndElement();//cbc:RegistrationName

				//--
				W.WriteStartElement("cac:RegistrationAddress");

				//W.WriteStartElement("cbc:ID");
				//W.WriteValue(oEmpresa.sCodigoUbigeoSunat);
				//W.WriteEndElement();//cbc:ID

				W.WriteStartElement("cbc:AddressTypeCode"); // Departamento
				W.WriteValue(oEmpresa.sEmpCodigoUbigeoSunat);
				W.WriteEndElement();//cbc:AddressTypeCode

				//W.WriteStartElement("cbc:CityName"); // Departamento
				//W.WriteValue(oEmpresa.sUbiNombre);
				//W.WriteEndElement();//cbc:CityName

				//W.WriteStartElement("cac:AddressLine"); // Departamento

				//W.WriteStartElement("cbc:Line"); // Departamento
				//W.WriteValue(oEmpresa.sEmpDireccion);
				//W.WriteEndElement();//cbc:Line

				//W.WriteEndElement();//cac:AddressLine

				W.WriteEndElement();//cac:RegistrationAddress
									//--

				W.WriteEndElement();//cac:PartyLegalEntity

				#endregion

				W.WriteEndElement(); //cac:Party         

				W.WriteEndElement(); //cac:AccountingSupplierParty

				W.WriteStartElement("cac:AccountingCustomerParty");
				W.WriteStartElement("cac:Party");

				W.WriteStartElement("cac:PartyIdentification");
				W.WriteStartElement("cbc:ID");
				W.WriteStartAttribute("schemeID");
				//SEGUN LOS CODIGOS DE SUNAT 1-> DNI Y 6->RUC
				if (oTransaccion.sCliNroIdentidad.Length == 8)
					W.WriteValue("1");
				else //SI ES RUC:NO SE DA PERO PODRI SER NECESARIO
					if (oTransaccion.sCliNroIdentidad.Length == 11)
					//SEGUN LOS CODIGOS DE SUNAT 1-> DNI Y 6->RUC
					W.WriteValue("6");

				W.WriteStartAttribute("schemeAgencyName");
				W.WriteValue("PE:SUNAT");

				W.WriteStartAttribute("schemeURI");
				W.WriteValue("urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo06");

				W.WriteEndAttribute();
				W.WriteValue(oTransaccion.sCliNroIdentidad);
				W.WriteEndElement();
				W.WriteEndElement();


				#region NOMBRE DEL CLIENTE Y DIRECCION

				//--------------------------------------------
				W.WriteStartElement("cac:PartyLegalEntity");

				W.WriteStartElement("cbc:RegistrationName");
				W.WriteValue(oTransaccion.sCliente);
				W.WriteEndElement();//RegistrationName

				W.WriteEndElement();//PartyLegalEntity
									//---------------------------------------------

				W.WriteEndElement();//Party

				W.WriteEndElement(); //cac:AccountingCustomerParty

				#endregion


				#region DETRACCION

				if (oTransaccion.nTraTieneDetraccion == Enumerador.ESTADO_ACTIVO)
				{
					//---
					W.WriteStartElement("cac:PaymentMeans");
					W.WriteStartElement("cbc:PaymentMeansCode");
					W.WriteValue("001");
					W.WriteEndElement();//cbc:PaymentMeansCode
					W.WriteStartElement("cac:PayeeFinancialAccount");
					W.WriteStartElement("cbc:ID");
					W.WriteValue(CTA_BANCO_NACION_DETRACCIONES);
					W.WriteEndElement();//cbc:ID
					W.WriteEndElement();//cac:PayeeFinancialAccount
					W.WriteEndElement();//cac:PaymentMeans
										//--

					//--
					W.WriteStartElement("cac:PaymentTerms");
					W.WriteStartElement("cbc:PaymentMeansID");
					W.WriteValue(oEmpresa.sEmpCodigoDetraccion);
					W.WriteEndElement();//cbc:PaymentMeansID

					W.WriteStartElement("cbc:PaymentPercent");
					W.WriteValue(String.Format(CultureInfo.InvariantCulture, "{0:0.00}", oTransaccion.nTraPctjDetraccion));
					W.WriteEndElement();//cbc:PaymentPercent

					W.WriteStartElement("cbc:Amount");
					W.WriteStartAttribute("currencyID");
					W.WriteValue("PEN");
					W.WriteEndAttribute();
					//Asegurando que siempre se envie la detraccion en moneda nacional(Regla de sunat)
					W.WriteValue(String.Format(CultureInfo.InvariantCulture,
						"{0:0.00}", (oTransaccion.nTraMontoDetraccion)
						));

					W.WriteEndElement(); //</cbc:PayableAmount>

					W.WriteEndElement();//cac:PaymentTerms
										//--
				}

				#endregion

				// ***** IGV
				W.WriteStartElement("cac:TaxTotal"); //Impuestos Globales

				W.WriteStartElement("cbc:TaxAmount"); //Importe total de un tributo para la factura
				W.WriteStartAttribute("currencyID");
				W.WriteValue(MONEDA);
				W.WriteEndAttribute();

				if (oTransaccion.nTraEsGratuito == Enumerador.ESTADO_ACTIVO)
				{
					W.WriteValue("0.00");
				}
				else
				{
					W.WriteValue(String.Format(CultureInfo.InvariantCulture,
						"{0:0.00}", oTransaccion.nTraMontoIGV));
				}


				W.WriteEndElement(); //cbc:TaxAmount
				W.WriteStartElement("cac:TaxSubtotal");

				W.WriteStartElement("cbc:TaxableAmount"); //Importe total de un tributo para la factura <cbc:TaxableAmount currencyID="PEN">200.00</cbc:TaxableAmount>
				W.WriteStartAttribute("currencyID");
				W.WriteValue(MONEDA);
				W.WriteEndAttribute();
				W.WriteValue(String.Format(CultureInfo.InvariantCulture, "{0:0.00}",
					oTransaccion.nTraSubTotalSinIGV));
				W.WriteEndElement(); //cbc:TaxableAmount

				W.WriteStartElement("cbc:TaxAmount"); //Importe total de un tributo para la factura
				W.WriteStartAttribute("currencyID"); //Importe explícito a tributar
				W.WriteValue(MONEDA);
				W.WriteEndAttribute();

				if (oTransaccion.nTraEsGratuito == Enumerador.ESTADO_ACTIVO)
				{
					W.WriteValue("0.00");
				}
				else
				{
					W.WriteValue(String.Format(CultureInfo.InvariantCulture, "{0:0.00}",
						oTransaccion.nTraMontoIGV));
				}

				W.WriteEndElement(); //cbc:TaxAmount  

				W.WriteStartElement("cac:TaxCategory");
				W.WriteStartElement("cac:TaxScheme");
				W.WriteStartElement("cbc:ID"); //Identificación del tributo según Catálogo No. 05
				W.WriteStartAttribute("schemeID");
				W.WriteValue("UN/ECE 5305");

				W.WriteStartAttribute("schemeAgencyID");
				W.WriteValue("6");
				W.WriteEndAttribute();

				if (oEmpresa.nEmpExonerado == Enumerador.ESTADO_ACTIVO)
				{
					W.WriteValue("9997");
				}
				else
				{
					if (oTransaccion.nTraEsGratuito == Enumerador.ESTADO_ACTIVO)
					{
						W.WriteValue("9996");
					}
					else
					if (oTransaccion.LstTransaccionDetalle[0].nTDeEstadoIGV
						== (int)Enumerador.FACTURA_ELECTRONICA.TIPO_IGV_EXONERADO)
					{
						W.WriteValue("9997");
					}
					else
						if (oTransaccion.LstTransaccionDetalle[0].nTDeEstadoIGV
					   == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_IGV_NO_GRABADO)
					{
						W.WriteValue("9998");
					}
					else
					{
						W.WriteValue("1000");
					}
				}


				W.WriteEndElement();
				W.WriteStartElement("cbc:Name"); //Nombre del Tributo (IGV, ISC)


				if (oTransaccion.nTraEsGratuito == Enumerador.ESTADO_ACTIVO)
				{
					W.WriteValue("GRA");
				}
				else

				/*SI LA EMPRSA ES NAFECTA AL IGV*/
				/*---------------------------------------------------*/
				if (oEmpresa.nEmpExonerado == Enumerador.ESTADO_ACTIVO)
				{
					W.WriteValue("EXO");
					W.WriteEndElement();
					W.WriteStartElement("cbc:TaxTypeCode"); //Código del Tipo de Tributo (UN/ECE 5153)
					W.WriteValue("VAT");
				}
				else
				{
					if (oTransaccion.LstTransaccionDetalle[0].nTDeEstadoIGV == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_IGV_EXONERADO)
					{
						W.WriteValue("EXO");
					}
					else
						if (oTransaccion.LstTransaccionDetalle[0].nTDeEstadoIGV == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_IGV_NO_GRABADO)
					{
						W.WriteValue("INA");
					}
					else
					{
						W.WriteValue("IGV");
					}

					W.WriteEndElement();
					W.WriteStartElement("cbc:TaxTypeCode"); //Código del Tipo de Tributo (UN/ECE 5153)

					if (oTransaccion.nTraEsGratuito == Enumerador.ESTADO_ACTIVO)
					{
						W.WriteValue("FRE");
					}
					else
					if (oTransaccion.LstTransaccionDetalle[0].nTDeEstadoIGV
					   == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_IGV_EXONERADO)
					{
						W.WriteValue("VAT");
					}
					else
						if (oTransaccion.LstTransaccionDetalle[0].nTDeEstadoIGV
					   == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_IGV_NO_GRABADO)
					{
						W.WriteValue("FRE");
					}
					else
					{
						W.WriteValue("VAT");
					}
				}

				W.WriteEndElement();//cbc:TaxTypeCode
				W.WriteEndElement();//TaxScheme
				W.WriteEndElement(); //cac:TaxCategory

				//--
				W.WriteEndElement(); //cac:TaxSubtotal


				#region ICBPER
				if (oTransaccion.nTraICBPER == Enumerador.ESTADO_ACTIVO)
				{
					/*COMPLEMENTARIO: SI ES QUE TIENE OTROS IMPUESTOS
					 LOGICA: SI EL CAMPO ICBPER=1 ENTONCES SE ESCRIBIRÁ
					 EN EL XML EL VALOR CORRESPONDIENTE
					 */
					W.WriteStartElement("cac:TaxSubtotal");
					//--------------------------------------------
					W.WriteStartElement("cbc:TaxAmount");
					W.WriteStartAttribute("currencyID"); //Importe explícito a tributar MONEDA
					W.WriteValue(MONEDA);
					W.WriteEndAttribute();

					//este monto debe de cuadrar con la sumatoria de ICBPER del detalle
					W.WriteValue(String.Format(CultureInfo.InvariantCulture, "{0:0.00}", oTransaccion.nTraMontoICBPER));
					W.WriteEndElement(); //cbc:TaxAmount  
										 //--------------------------------------------
					W.WriteStartElement("cac:TaxCategory");
					W.WriteStartElement("cac:TaxScheme");
					W.WriteStartElement("cbc:ID"); //Identificación del tributo según Catálogo No. 05
					W.WriteValue("7152");//MODIFICACION DE CODIGO CON RESPECTO AL IMPUESTO A LAS BOLSAS
					W.WriteEndElement();
					W.WriteStartElement("cbc:Name");
					W.WriteValue("ICBPER");
					W.WriteEndElement();
					W.WriteStartElement("cbc:TaxTypeCode");
					W.WriteValue("OTH");
					W.WriteEndElement();//cbc:TaxTypeCode
					W.WriteEndElement();//cac:TaxScheme
					W.WriteEndElement(); //cac:TaxCategory
										 //--
					W.WriteEndElement(); //cac:TaxSubtotal ICBPER
										 /*--------------------------------------------------*/
				}

				#endregion


				W.WriteEndElement(); //cac:TaxTotal

				W.WriteStartElement("cac:LegalMonetaryTotal"); //Totales a pagar de la Factura y Cargos

				if (
					 oTransaccion.TipoDocumento_Id ==
					 (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_FACTURA ||
					 oTransaccion.TipoDocumento_Id ==
					 (int)Enumerador.FACTURA_ELECTRONICA.TIPO_DOCUMENTO_BOLETA
					 )
				{
					W.WriteStartElement("cbc:TaxInclusiveAmount"); //<cbc:TaxInclusiveAmount>
					W.WriteStartAttribute("currencyID");
					W.WriteValue(MONEDA);
					W.WriteEndAttribute();

					if (oTransaccion.nTraEsGratuito == Enumerador.ESTADO_ACTIVO)
						W.WriteValue("0.00");
					else
						W.WriteValue(String.Format(CultureInfo.InvariantCulture,
							"{0:0.00}", oTransaccion.nTraSubTotalSinIGV));

					W.WriteEndElement(); //cbc:TaxInclusiveAmount

				}

				W.WriteStartElement("cbc:PayableAmount");
				W.WriteStartAttribute("currencyID");
				W.WriteValue(MONEDA);
				W.WriteEndAttribute();

				if (oTransaccion.nTraEsGratuito == Enumerador.ESTADO_ACTIVO)
					W.WriteValue("0.00");
				else
					W.WriteValue(String.Format(CultureInfo.InvariantCulture,
						"{0:0.00}", oTransaccion.nTraImporteTotal));


				W.WriteEndElement(); //cbc:PayableAmount
				W.WriteEndElement(); //cac:LegalMonetaryTotal

				GenerarDetalleBoletaXmlUBL2_1(oTransaccion, W);


				// ***** Copiando el XML *****
				W.WriteEndElement(); // Cerrando Nodos Principales
									 // ***** Grabando datos del XML *****
				W.Flush();
				W.Close();

				#region  INCRUSTAR CERTIFICADO

				Certificado(strNombreArchivo,
								  oTransaccion.TipoDocumento_Id,
								  p_oEmpresa, Enumerador.ESTADO_ACTIVO
								 );

				#endregion

				#region COMPRIMIR EL ARCHIVO XML FORMADO A ZIP

				ComprimirXML(strNombreArchivo, oTransaccion.TipoDocumento_Id, p_oEmpresa);

				#endregion


				return true;
			}
			catch (Exception)
			{
				throw;
			}
		}


		/// <summary>
		/// genera el detalle de la boleta eletronica
		/// </summary>
		/// <param name="oTransaccion"></param>
		/// <param name="W"></param>
		private void GenerarDetalleBoletaXmlUBL2_1(Transaccion oTransaccion, XmlTextWriter W)
		{
			try
			{
				int i = 0;
				decimal nCantidadProducto = 0;
				string MONEDA = oTransaccion.Moneda_Id == Enumerador.MONEDA_NACIONAL_SOLES_ID ? "PEN" : "USD";

				foreach (var item in oTransaccion.LstTransaccionDetalle)
				{
					i++;

					nCantidadProducto = 0;
					nCantidadProducto = item.nTDeCantidad;

					W.WriteStartElement("cac:InvoiceLine"); //Ítems de Factura
					W.WriteStartElement("cbc:ID"); // Número de orden del Ítem
					W.WriteValue(i);
					W.WriteEndElement();
					W.WriteStartElement("cbc:InvoicedQuantity"); // Unidad de medida por Ítem (UN/ECE rec 20)
					W.WriteStartAttribute("unitCode");
					W.WriteValue(item.sUMeCodigoSunat);

					W.WriteStartAttribute("unitCodeListID");
					W.WriteValue("UN/ECE rec 20");

					W.WriteStartAttribute("unitCodeListAgencyName");
					W.WriteValue("United Nations Economic Commission for Europe");

					W.WriteEndAttribute();
					W.WriteValue(String.Format(CultureInfo.InvariantCulture,
						   "{0:0.00}", nCantidadProducto));
					W.WriteEndElement();

					///////////

					W.WriteStartElement("cbc:LineExtensionAmount");
					W.WriteStartAttribute("currencyID");
					W.WriteValue(MONEDA);
					W.WriteEndAttribute();

					if (oTransaccion.nTraEsGratuito == Enumerador.ESTADO_ACTIVO)
					{
						W.WriteValue("0.00");
					}
					else
						W.WriteValue(String.Format(CultureInfo.InvariantCulture, "{0:0.00}", item.nTDeBase));

					W.WriteEndElement();//LineExtensionAmount

					W.WriteStartElement("cac:PricingReference"); //Valores unitarios

					if (oTransaccion.nTraEsGratuito == Enumerador.ESTADO_ACTIVO)
					{
						W.WriteStartElement("cac:AlternativeConditionPrice"); //Valores unitarios
						W.WriteStartElement("cbc:PriceAmount"); //Monto del valor unitario
						W.WriteStartAttribute("currencyID");
						W.WriteValue(MONEDA);
						W.WriteEndAttribute();
						W.WriteValue(String.Format(CultureInfo.InvariantCulture,
							"{0:0.00}", item.nTDePrecio));
						W.WriteEndElement();
						W.WriteStartElement("cbc:PriceTypeCode"); //Código del valor unitario
						W.WriteValue("02");
						W.WriteEndElement();
					}
					else
					{
						W.WriteStartElement("cac:AlternativeConditionPrice"); //Valores unitarios
						W.WriteStartElement("cbc:PriceAmount"); //Monto del valor unitario
						W.WriteStartAttribute("currencyID");
						//Importe explícito a tributar Moneda e Importe total a pagar
						W.WriteValue(MONEDA);
						W.WriteEndAttribute();
						W.WriteValue(String.Format(CultureInfo.InvariantCulture,
							"{0:0.00}", item.nTDePrecio));
						W.WriteEndElement();
						W.WriteStartElement("cbc:PriceTypeCode"); //Código del valor unitario
						W.WriteValue("01");
						W.WriteEndElement();//cbc:PriceTypeCode"
					}

					W.WriteEndElement();// </cac:AlternativeConditionPrice>

					W.WriteEndElement(); // </cac:PricingReference>

					if (oTransaccion.nTraEsGratuito == Enumerador.ESTADO_ACTIVO)
					{
						// ***** Igv Total del Documento *****
						W.WriteStartElement("cac:TaxTotal");
						W.WriteStartElement("cbc:TaxAmount"); //Importe total de un tributo para este ítem
						W.WriteStartAttribute("currencyID");
						W.WriteValue(MONEDA);
						W.WriteEndAttribute();
						W.WriteValue(String.Format(CultureInfo.InvariantCulture,
							"{0:0.00}", item.nTDeIGV));
						W.WriteEndElement();

						// ***** Igv x Producto *****
						W.WriteStartElement("cac:TaxSubtotal");

						W.WriteStartElement("cbc:TaxableAmount");
						W.WriteStartAttribute("currencyID");
						W.WriteValue(MONEDA);
						W.WriteEndAttribute();
						W.WriteValue(String.Format(CultureInfo.InvariantCulture,
							"{0:0.00}", item.nTDeBase));
						W.WriteEndElement();

						W.WriteStartElement("cbc:TaxAmount");
						W.WriteStartAttribute("currencyID");
						W.WriteValue(MONEDA);
						W.WriteEndAttribute();
						W.WriteValue(String.Format(CultureInfo.InvariantCulture,
							"{0:0.00}", item.nTDeIGV));
						W.WriteEndElement();

						W.WriteStartElement("cac:TaxCategory"); //Afectación del IGV (Catálogo No. 07)

						W.WriteStartElement("cbc:Percent"); //Porcentaje de Sunat aplicado
						W.WriteValue("18");//por defecto
						W.WriteEndElement();

						W.WriteStartElement("cbc:TaxExemptionReasonCode");

						W.WriteStartAttribute("listAgencyName");
						W.WriteValue("PE:SUNAT");

						//W.WriteStartAttribute("listName");
						//W.WriteValue("Afectacion del IGV");

						//W.WriteStartAttribute("listURI");
						//W.WriteValue("urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo07");

						W.WriteEndAttribute();

						W.WriteValue("11");
						W.WriteEndElement();//TaxExemptionReasonCode

						W.WriteStartElement("cbc:TaxScheme");

						W.WriteStartElement("cbc:ID"); //Identificación del tributo según Catálogo No. 05

						W.WriteStartAttribute("schemeID");
						W.WriteValue("UN/ECE 5153");

						//W.WriteStartAttribute("schemeName");
						//W.WriteValue("Tax Scheme Identifier");

						//W.WriteStartAttribute("schemeAgencyName");
						//W.WriteValue("United Nations Economic Commission for Europe");
						W.WriteEndAttribute();

						W.WriteValue("9996");
						W.WriteEndElement();

						W.WriteStartElement("cbc:Name"); //Identificación del tributo según Catálogo No. 05
						W.WriteValue("GRA");
						W.WriteEndElement();

						W.WriteStartElement("cbc:TaxTypeCode");
						W.WriteValue("FRE");
						W.WriteEndElement();//cbc:TaxTypeCode

						W.WriteEndElement();//TaxScheme
					}
					else
					{
						// ***** Igv Total del Documento *****
						W.WriteStartElement("cac:TaxTotal");
						W.WriteStartElement("cbc:TaxAmount"); //Importe total de un tributo para este ítem
						W.WriteStartAttribute("currencyID");
						W.WriteValue(MONEDA);
						W.WriteEndAttribute();
						W.WriteValue(String.Format(CultureInfo.InvariantCulture,
							"{0:0.00}", item.nTDeIGV));
						W.WriteEndElement();

						// ***** Igv x Producto *****
						W.WriteStartElement("cac:TaxSubtotal");

						W.WriteStartElement("cbc:TaxableAmount");
						W.WriteStartAttribute("currencyID");
						W.WriteValue(MONEDA);
						W.WriteEndAttribute();
						W.WriteValue(String.Format(CultureInfo.InvariantCulture,
							"{0:0.00}", item.nTDeBase));
						W.WriteEndElement();

						W.WriteStartElement("cbc:TaxAmount");
						W.WriteStartAttribute("currencyID");
						W.WriteValue(MONEDA);
						W.WriteEndAttribute();
						W.WriteValue(String.Format(CultureInfo.InvariantCulture,
							"{0:0.00}", item.nTDeIGV));
						W.WriteEndElement();

						W.WriteStartElement("cac:TaxCategory");
						W.WriteStartElement("cbc:Percent"); //Porcentaje de Sunat aplicado
						W.WriteValue("18");//por defecto
						W.WriteEndElement();

						W.WriteStartElement("cbc:TaxExemptionReasonCode");

						W.WriteStartAttribute("listAgencyName");
						W.WriteValue("PE:SUNAT");

						//W.WriteStartAttribute("listName");
						//W.WriteValue("Afectacion del IGV");

						//W.WriteStartAttribute("listURI");
						//W.WriteValue("urn:pe:gob:sunat:cpe:see:gem:catalogos:catalogo07");

						W.WriteEndAttribute();

						if (oEmpresa.nEmpExonerado == Enumerador.ESTADO_ACTIVO)
						{
							W.WriteValue("20");
						}
						else
							if (item.nTDeEstadoIGV == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_IGV_GRABADO)
							W.WriteValue("10");
						else
									if (item.nTDeEstadoIGV == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_IGV_NO_GRABADO)
						{
							W.WriteValue("30"); //INAFECTO OPERACION
						}
						else//EXONERADO DEL IGV
									if (item.nTDeEstadoIGV == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_IGV_EXONERADO)
						{
							W.WriteValue("20");
						}


						W.WriteEndElement();

						W.WriteStartElement("cac:TaxScheme");
						W.WriteStartElement("cbc:ID"); //Identificación del tributo según Catálogo No. 05

						W.WriteStartAttribute("schemeID");
						W.WriteValue("UN/ECE 5153");

						//W.WriteStartAttribute("schemeName");
						//W.WriteValue("Tax Scheme Identifier");

						//W.WriteStartAttribute("schemeAgencyName");
						//W.WriteValue("United Nations Economic Commission for Europe");
						W.WriteEndAttribute();

						if (oEmpresa.nEmpExonerado == Enumerador.ESTADO_ACTIVO)
						{
							W.WriteValue("9997");
						}
						else
						if (item.nTDeEstadoIGV ==
							   (int)Enumerador.FACTURA_ELECTRONICA.TIPO_IGV_EXONERADO)
						{
							W.WriteValue("9997");
						}
						else
							   if (item.nTDeEstadoIGV ==
							   (int)Enumerador.FACTURA_ELECTRONICA.TIPO_IGV_NO_GRABADO)
						{
							W.WriteValue("9998");//INAFECTO
						}
						else
						{
							W.WriteValue("1000");
						}


						W.WriteEndElement();

						W.WriteStartElement("cbc:Name"); //Nombre del Tributo (IGV, ISC)

						if (oEmpresa.nEmpExonerado == Enumerador.ESTADO_ACTIVO)
						{
							W.WriteValue("EXO");
						}
						else
					if (item.nTDeEstadoIGV ==
							   (int)Enumerador.FACTURA_ELECTRONICA.TIPO_IGV_EXONERADO)
						{
							W.WriteValue("EXO");
						}
						else
								 if (item.nTDeEstadoIGV ==
							   (int)Enumerador.FACTURA_ELECTRONICA.TIPO_IGV_NO_GRABADO)
						{
							W.WriteValue("INA");
						}
						else
						{
							W.WriteValue("IGV");
						}

						W.WriteEndElement();

						W.WriteStartElement("cbc:TaxTypeCode"); //Código del Tipo de Tributo (UN/ECE 5153)
						if (item.nTDeEstadoIGV == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_IGV_NO_GRABADO)
						{
							W.WriteValue("FRE");
						}
						else
							W.WriteValue("VAT");
						W.WriteEndElement();
						W.WriteEndElement();
					}

					W.WriteEndElement(); //cac:TaxCategory
					W.WriteEndElement(); //cac:TaxSubtotal

					//-----------------------------------------------------
					#region ICBPER
					/*en el caso de que se haya recibido este campo se debe
                     * escribir en el xml
                     * 
                    */
					if (item.nTDeICBPER > 0)
					{
						//--TaxSubtotal
						W.WriteStartElement("cac:TaxSubtotal");

						//--TaxAmount
						W.WriteStartElement("cbc:TaxAmount");
						W.WriteStartAttribute("currencyID");
						W.WriteValue(MONEDA);
						W.WriteEndAttribute();
						W.WriteValue(String.Format(CultureInfo.InvariantCulture, "{0:0.00}", item.nTDeICBPER));
						W.WriteEndElement(); //cbc:TaxAmount

						//--BaseUnitMeasure
						W.WriteStartElement("cbc:BaseUnitMeasure");
						W.WriteStartAttribute("unitCode");
						W.WriteValue(MONEDA);
						W.WriteEndAttribute();
						//OJO: La regla dice que este campo debe enviarse como entero
						//por eso la conversion
						W.WriteValue(item.nTDeCantidad.toInt());
						W.WriteEndElement(); //cbc:BaseUnitMeasure

						//--cac:TaxCategory
						W.WriteStartElement("cac:TaxCategory");


						//--cbc:PerUnitAmount
						W.WriteStartElement("cbc:PerUnitAmount");
						W.WriteStartAttribute("currencyID");
						W.WriteValue(MONEDA);
						W.WriteEndAttribute();
						//OJO: La regla dice que este campo debe enviarse como entero
						//por eso la conversion
						W.WriteValue(item.nTDeICBPER);
						W.WriteEndElement(); //cbc:PerUnitAmount


						//--cac:TaxScheme
						W.WriteStartElement("cac:TaxScheme");

						//--cbc:ID
						W.WriteStartElement("cbc:ID");
						//Identificación del tributo según Catálogo No. 05
						//codigo del ICBPER SEGUN SUNAT
						W.WriteValue("7152");
						W.WriteEndElement();//cbc:ID

						//--cbc:Name
						W.WriteStartElement("cbc:Name"); //Nombre del Tributo (IGV, ISC)
						W.WriteValue("ICBPER");
						W.WriteEndElement();//cbc:Name

						//--cbc:TaxTypeCode
						W.WriteStartElement("cbc:TaxTypeCode"); //Código del Tipo de Tributo (UN/ECE 5153)
						W.WriteValue("OTH");
						W.WriteEndElement();//cbc:TaxTypeCode

						W.WriteEndElement();//cac:TaxScheme

						W.WriteEndElement(); //cac:TaxCategory

						W.WriteEndElement(); //cac:TaxSubtotal
					}

					#endregion
					//------------------------------------------------------

					W.WriteEndElement(); //cac:TaxTotal

					W.WriteStartElement("cac:Item");
					W.WriteStartElement("cbc:Description");
					W.WriteValue(item.sProNombre);
					W.WriteEndElement();

					W.WriteStartElement("cac:SellersItemIdentification"); //Identificador de elementos de ítem
					W.WriteStartElement("cbc:ID"); //Código del producto
					W.WriteValue(item.Producto_Id);
					W.WriteEndElement();
					W.WriteEndElement();
					W.WriteEndElement(); //cac:Item
					W.WriteStartElement("cac:Price");
					W.WriteStartElement("cbc:PriceAmount");
					W.WriteStartAttribute("currencyID"); //Importe explícito a tributar Moneda e Importe total a pagar
					W.WriteValue(MONEDA);
					W.WriteEndAttribute();

					if (oTransaccion.nTraEsGratuito == Enumerador.ESTADO_ACTIVO)
						W.WriteValue("0.00");
					else
						W.WriteValue(String.Format(CultureInfo.InvariantCulture,
							"{0:0.00}", item.nTDePrecio));

					W.WriteEndElement();
					W.WriteEndElement(); //cac:Price

					W.WriteEndElement(); // cac:InvoiceLine Final de Productos
				}
			}
			catch (Exception)
			{
				throw;
			}
		}



		/*LIQUIDACION  DE COMPRA*/

		public bool GenerarArchivoLiquidacionCompraXmlyZip(Transaccion oTransaccion, Empresa p_oEmpresa)
		{
			try
			{
				CTA_BANCO_NACION_DETRACCIONES = p_oEmpresa.sEmpCtaBancoDetracciones.Replace("-", "");
				oEmpresa = p_oEmpresa;

				#region CREAR DOCUMENTO ELECTRONICO

				#region INICIALES

				strNombreArchivo = GenerarNombreXML(oTransaccion, oEmpresa);

				string sMoneda = string.Empty;
				//Texto para mostrar en el importe pasado a letras
				if (oTransaccion.Moneda_Id == Enumerador.MONEDA_EXTRANJERA_DOLARES_ID)
					sMoneda = @"Dolares Americanos";
				else
					sMoneda = @"Soles";

				string CODIGO_DOCUMENTO_SUNAT = string.Empty;

				//texto para mostrar CURRENCYID-> MONEDA EN PEN O USD
				string MONEDA = oTransaccion.Moneda_Id == Enumerador.MONEDA_NACIONAL_SOLES_ID ? "PEN" : "USD";

				#endregion

				#region CREAR DIRECTORIO SI NO EXISTE

				CrearDirectorio(oTransaccion.TipoDocumento_Id, strNombreArchivo, oEmpresa);

				#endregion

				#region PREPARAR EL ESCRITOR DE XML

				XmlTextWriter W = null;
				
				#region FACTURAS

					W =
						new XmlTextWriter(oEmpresa.sEmpRuta +
							RUTA_REPOSITORIO_ELECTRONICO_LIQUIDACION_COMPRA + strNombreArchivo + @"\" + strNombreArchivo +
							".xml",
							Encoding.GetEncoding("ISO-8859-1"));

				#endregion

				W.Formatting = Formatting.Indented;
				W.WriteStartDocument(false);
				#endregion

				#region INICIO DE DOCUMENTO

				#region 02 - Liquidacion de Compra - > sunat 04

				W.WriteStartElement("SelfBilledInvoice"); //  Documento utilizado para requerir un pago
				W.WriteStartAttribute("xmlns");
				W.WriteValue("urn:oasis:names:specification:ubl:schema:xsd:SelfBilledInvoice-2");
				CODIGO_DOCUMENTO_SUNAT = "04";

				#endregion

				#endregion

				#region NECESARIOS SEGUN MODELO SUNAT XML

				W.WriteStartAttribute("xmlns:cac");
				W.WriteValue("urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
				W.WriteStartAttribute("xmlns:cbc");
				W.WriteValue("urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
				W.WriteStartAttribute("xmlns:ds");
				W.WriteValue("http://www.w3.org/2000/09/xmldsig#");
				W.WriteStartAttribute("xmlns:ext");
				W.WriteValue("urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2");
				W.WriteEndAttribute();

				#endregion

				//CUERPO PRINCIPAL DE CONTENIDO
				/*===============================================*/
				W.WriteStartElement("ext:UBLExtensions");
				/*===============================================*/

				//EXTENSION PARA INCRUSTAR LA FIRMA DIGITAL*/
				//==================================================================
				/*  Contenedor de Componentes de extensión. 
                    Podrán incorporarse nuevas definiciones estructuradas 
                    cuando sean de interés conjunto para emisores y receptores, 
                    y no estén ya definidas en el esquema de la factura. */

				W.WriteStartElement("ext:UBLExtension");
				W.WriteStartElement("ext:ExtensionContent");
				W.WriteEndElement();
				W.WriteEndElement();//ext:UBLExtension

				W.WriteEndElement(); //</ext:UBLExtension>

				/*--FIN DE EXTENSIONES UBL*/
				W.WriteStartElement("cbc:UBLVersionID"); //Versión del UBL
				W.WriteValue("2.1");
				W.WriteEndElement();
				W.WriteStartElement("cbc:CustomizationID"); //Versión de la estructura del documento
				W.WriteValue("2.0");
				W.WriteEndElement();
				W.WriteStartElement("cbc:ID");
				W.WriteValue(oTransaccion.sNumeroGenerado);
				W.WriteEndElement();
				W.WriteStartElement("cbc:IssueDate");
				W.WriteValue(oTransaccion.dTraFecha.ToString("yyyy-MM-dd"));
				W.WriteEndElement();

				W.WriteStartElement("cbc:InvoiceTypeCode");
				W.WriteStartAttribute("listID");
				W.WriteValue("0501");

				W.WriteEndAttribute();
				W.WriteValue(CODIGO_DOCUMENTO_SUNAT);
				W.WriteEndElement(); //cbc:PayableAmount

				W.WriteStartElement("cbc:Note");
				W.WriteStartAttribute("languageLocaleID");
				W.WriteValue("1000");
				W.WriteEndAttribute();
				W.WriteValue(Util.ConvertirNumerosAletras(oTransaccion.nTraImporteTotal, sMoneda));
				W.WriteEndElement(); //cbc:Note

				W.WriteStartElement("cbc:DocumentCurrencyCode");
				W.WriteValue(MONEDA);
				W.WriteEndElement();

				#region REFERENCIA A LA FIRMA DIGITAL

				/*Nota: Solo anteponer la letra "S" 
                * luego el numero completo serienumero del correlativo de la transaccion*/

				W.WriteStartElement("cac:Signature"); //Referencia a la Firma Digital
				W.WriteStartElement("cbc:ID"); //Identificador de la firma
				W.WriteValue("S" + oTransaccion.sNumeroGenerado);
				W.WriteEndElement();

				#endregion

				// ***** Datos Empresa certificado ******
				W.WriteStartElement("cac:SignatoryParty"); //Código del tipo de documento adicional (p.e. SCOP)

				W.WriteStartElement("cac:PartyIdentification"); //Parte firmante

				W.WriteStartElement("cbc:ID"); //Identificación de la parte firmante
											   //AGREGAR EL RUC DE LA EMPRESA EMISORA
				W.WriteValue(oEmpresa.sEmpRuc);
				W.WriteEndElement();//cbc:ID

				W.WriteEndElement();//PartyIdentification

				W.WriteStartElement("cac:PartyName");
				W.WriteStartElement("cbc:Name"); //Nombre de la parte firmante
				W.WriteValue(oEmpresa.sEmpNombre);
				W.WriteEndElement();//Name
				W.WriteEndElement();//PartyName

				W.WriteEndElement();//SignatoryParty

				W.WriteStartElement("cac:DigitalSignatureAttachment");
				W.WriteStartElement("cac:ExternalReference");
				W.WriteStartElement("cbc:URI");
				//Identificador de Recurso Uniforme (o URI) que identifica la localización de la firma
				/*Nota: Solo anteponer el caracter "#" 
                * luego el numero completo serienumero del correlativo de la transaccion*/
				W.WriteValue("#" + oTransaccion.sNumeroGenerado);
				W.WriteEndElement();
				W.WriteEndElement();
				W.WriteEndElement();
				W.WriteEndElement(); //cac:Signature
									 //nuevamente

				#region NOMBRE DEL EMISOR COMPRADOR
				W.WriteStartElement("cac:AccountingCustomerParty");
				W.WriteStartElement("cac:Party");
				W.WriteStartElement("cac:PartyIdentification");
				W.WriteStartElement("cbc:ID"); //Nombre Comercial
				W.WriteStartAttribute("schemeID");
				//SEGUN LOS CODIGOS DE SUNAT 1-> DNI Y 6->RUC
				
				if (oEmpresa.sEmpRuc.Length == 8)
					W.WriteValue("1");
				else
					W.WriteValue("6");

				W.WriteEndAttribute();
				W.WriteValue(oEmpresa.sEmpRuc);
				W.WriteEndElement();
				W.WriteEndElement();
				W.WriteStartElement("cac:PartyLegalEntity");
				W.WriteStartElement("cbc:RegistrationName");
				W.WriteValue(oEmpresa.sEmpNombre);
				W.WriteEndElement();//RegistrationName
				W.WriteStartElement("cac:RegistrationAddress");
				W.WriteStartElement("cac:AddressLine");
				W.WriteStartElement("cbc:Line");
				W.WriteValue(oEmpresa.sEmpDireccion);
				W.WriteEndElement();//RegistrationName
				W.WriteEndElement();//AddressLine
				W.WriteStartElement("cac:Country");
				W.WriteStartElement("cbc:IdentificationCode");
				W.WriteValue(oEmpresa.sEmpAbreviaturaPais);
				W.WriteEndElement();//cbc:IdentificationCode
				W.WriteEndElement();//cac:Country
				W.WriteEndElement();//RegistrationAddress
				W.WriteEndElement();//PartyLegalEntity
				W.WriteEndElement();//Party
				W.WriteEndElement(); //cac:AccountingCustomerParty
				#endregion

				#region NOMBRE DEL PROVEEDOR VENDEDOR
				W.WriteStartElement("cac:AccountingSupplierParty"); //Datos del Emisor del documento
				W.WriteStartElement("cac:Party");
				W.WriteStartElement("cac:PartyIdentification");
				W.WriteStartElement("cbc:ID");
				W.WriteStartAttribute("schemeID");

				//SEGUN LOS CODIGOS DE SUNAT 1-> DNI Y 6->RUC
				if (oTransaccion.nCliImportacion == Enumerador.ESTADO_ACTIVO)
					W.WriteValue("0");
				else
				if (oTransaccion.sCliNroIdentidad.Length == 8)
					W.WriteValue("1");

				W.WriteEndAttribute();
				W.WriteValue(oTransaccion.sCliNroIdentidad);
				W.WriteEndElement();//cbc:ID
				W.WriteEndElement();//cac:PartyIdentification
				W.WriteStartElement("cac:PartyName");
				W.WriteStartElement("cbc:Name"); //Nombre Comercial
				W.WriteValue(oTransaccion.sCliente);
				W.WriteEndElement();//cbc:Name
				W.WriteEndElement();//PartyName
				W.WriteStartElement("cac:PartyLegalEntity");
				W.WriteStartElement("cbc:RegistrationName");
				W.WriteValue(oTransaccion.sCliente);
				W.WriteEndElement();//cbc:RegistrationName
				W.WriteStartElement("cac:RegistrationAddress");
				W.WriteStartElement("cbc:ID");
				W.WriteValue(oEmpresa.sEmpCodigoUbigeoSunat);
				W.WriteEndElement();//cbc:ID
				
				///TIPO  DE DOMICILIO DEL VENDEDOR CATALOGO 60
				///=========================================================
				W.WriteStartElement("cbc:AddressTypeCode");
				W.WriteValue("01");
				///=========================================================

				W.WriteEndElement();//cbc:AddressTypeCode
				W.WriteStartElement("cbc:CityName"); // Departamento
				W.WriteValue(oEmpresa.sEmpCiudad);
				W.WriteEndElement();//cbc:CityName
				W.WriteStartElement("cac:AddressLine"); // Departamento
				W.WriteStartElement("cbc:Line"); // Departamento
				W.WriteValue(oTransaccion.sCliDomicilioFiscal);
				W.WriteEndElement();//cbc:Line
				W.WriteEndElement();//cac:AddressLine
				W.WriteEndElement();//cac:RegistrationAddress
				W.WriteEndElement();//cac:PartyLegalEntity
				W.WriteEndElement(); //cac:Party         
				W.WriteEndElement(); //cac:AccountingSupplierParty
				#endregion


				#region UBICACION DONDE SE REALIZA LA OPERACION

				W.WriteStartElement("cac:DeliveryTerms");
				W.WriteStartElement("cac:DeliveryLocation");

				W.WriteStartElement("cbc:LocationTypeCode"); // cbc:LocationTypeCode
				W.WriteValue("01"); // CATALOGO 60
				W.WriteEndElement();//cbc:LocationTypeCode

				//-----ADDRESSS
				//==============================================================
				W.WriteStartElement("cac:Address");

				W.WriteStartElement("cbc:ID"); //ID
				W.WriteStartAttribute("schemeAgencyName");
				W.WriteValue("PE:INEI");
				W.WriteStartAttribute("schemeName");
				W.WriteValue("Ubigeos");
				W.WriteEndAttribute();
				W.WriteValue(oEmpresa.sEmpCodigoUbigeoSunat);
				W.WriteEndElement();//ID
				
				W.WriteStartElement("cbc:CityName"); // CIUDAD
				W.WriteValue("Lima");
				W.WriteEndElement();//cbc:District

				W.WriteStartElement("cbc:CountrySubentity"); // CIUDAD
				W.WriteValue("Lima");
				W.WriteEndElement();//cbc:District

				W.WriteStartElement("cbc:District"); // CIUDAD
				W.WriteValue("Lima");
				W.WriteEndElement();//cbc:District
				
				//----
				W.WriteStartElement("cac:AddressLine"); 
				W.WriteStartElement("cbc:Line"); 
				W.WriteValue(oEmpresa.sEmpDireccion);
				W.WriteEndElement();//cbc:Line
				W.WriteEndElement();//cac:AddressLine

				
				W.WriteStartElement("cac:Country"); // CIUDAD

				W.WriteStartElement("cbc:IdentificationCode"); // CODIGO DEL PAIS
				W.WriteValue("PE");
				W.WriteEndElement();//cbc:IdentificationCode

				W.WriteEndElement();//cac:Country


				W.WriteEndElement();//cac:Address
				//==============================================================

				
				
				W.WriteEndElement();//cac:DeliveryLocation
				
				W.WriteEndElement(); //cac:DeliveryTerms
				
				#endregion

				// ***** IGV
				W.WriteStartElement("cac:TaxTotal"); //Impuestos Globales

				W.WriteStartElement("cbc:TaxAmount"); //Importe total de un tributo para la factura
				W.WriteStartAttribute("currencyID");
				W.WriteValue(MONEDA);
				W.WriteEndAttribute();
				
				W.WriteValue(String.Format(CultureInfo.InvariantCulture, "{0:0.00}", oTransaccion.nTraMontoIGV));

				W.WriteEndElement(); //cbc:TaxAmount
				W.WriteStartElement("cac:TaxSubtotal");

				W.WriteStartElement("cbc:TaxableAmount"); //Importe total de un tributo para la factura <cbc:TaxableAmount currencyID="PEN">200.00</cbc:TaxableAmount>
				W.WriteStartAttribute("currencyID");
				W.WriteValue(MONEDA);
				W.WriteEndAttribute();
				W.WriteValue(String.Format(CultureInfo.InvariantCulture, "{0:0.00}", oTransaccion.nTraSubTotalSinIGV));
				W.WriteEndElement(); //cbc:TaxableAmount

				W.WriteStartElement("cbc:TaxAmount"); //Importe total de un tributo para la factura
				W.WriteStartAttribute("currencyID"); //Importe explícito a tributar
				W.WriteValue(MONEDA);
				W.WriteEndAttribute();
				
				W.WriteValue(String.Format(CultureInfo.InvariantCulture, "{0:0.00}", oTransaccion.nTraMontoIGV));

				W.WriteEndElement(); //cbc:TaxAmount  

				W.WriteStartElement("cac:TaxCategory");
				W.WriteStartElement("cac:TaxScheme");
				W.WriteStartElement("cbc:ID"); //Identificación del tributo según Catálogo No. 05


				if (oEmpresa.nEmpExonerado == Enumerador.ESTADO_ACTIVO)
				{
					W.WriteValue("9997");
				}
				else
				{
					if (oTransaccion.nTraEsGratuito == Enumerador.ESTADO_ACTIVO)
					{
						W.WriteValue("9996");
					}
					else
					if (oTransaccion.nCliImportacion == Enumerador.ESTADO_ACTIVO)
					{
						W.WriteValue("9995");
					}

					else
					if (oTransaccion.LstTransaccionDetalle[0].nTDeEstadoIGV == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_IGV_EXONERADO)
					{
						W.WriteValue("9997");
					}
					else
						if (oTransaccion.LstTransaccionDetalle[0].nTDeEstadoIGV
					   == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_IGV_NO_GRABADO)
					{
						W.WriteValue("9998");
					}
					else
					{
						W.WriteValue("1000");
					}
				}

				W.WriteEndElement();
				W.WriteStartElement("cbc:Name"); //Nombre del Tributo (IGV, ISC)


				if (oTransaccion.nTraEsGratuito == Enumerador.ESTADO_ACTIVO)
				{
					W.WriteValue("GRA");
				}
				else
				if (oTransaccion.nCliImportacion == Enumerador.ESTADO_ACTIVO)
				{
					W.WriteValue("EXP");
				}
				else

				/*SI LA EMPRSA ES NAFECTA AL IGV*/
				/*---------------------------------------------------*/
				if (oEmpresa.nEmpExonerado == Enumerador.ESTADO_ACTIVO)
				{
					W.WriteValue("EXO");
					W.WriteEndElement();
					W.WriteStartElement("cbc:TaxTypeCode"); //Código del Tipo de Tributo (UN/ECE 5153)
					W.WriteValue("VAT");
				}
				else
				{
					if (oTransaccion.LstTransaccionDetalle[0].nTDeEstadoIGV == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_IGV_EXONERADO)
					{
						W.WriteValue("EXO");
					}
					else
						if (oTransaccion.LstTransaccionDetalle[0].nTDeEstadoIGV
					   == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_IGV_NO_GRABADO)
					{
						W.WriteValue("INA");
					}
					else
					{
						W.WriteValue("IGV");
					}


					W.WriteEndElement();
					W.WriteStartElement("cbc:TaxTypeCode"); //Código del Tipo de Tributo (UN/ECE 5153)


					if (oTransaccion.nCliImportacion == Enumerador.ESTADO_ACTIVO)
					{
						W.WriteValue("FRE");
					}
					else
					if (oTransaccion.nTraEsGratuito == Enumerador.ESTADO_ACTIVO)
					{
						W.WriteValue("FRE");
					}
					else
					if (oTransaccion.LstTransaccionDetalle[0].nTDeEstadoIGV
					   == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_IGV_EXONERADO)
					{
						W.WriteValue("VAT");
					}
					else
						if (oTransaccion.LstTransaccionDetalle[0].nTDeEstadoIGV
					   == (int)Enumerador.FACTURA_ELECTRONICA.TIPO_IGV_NO_GRABADO)
					{
						W.WriteValue("FRE");
					}
					else
					{
						W.WriteValue("VAT");
					}
				}

				W.WriteEndElement();//cbc:TaxTypeCode
				W.WriteEndElement();
				W.WriteEndElement(); //cac:TaxCategory

				//--
				W.WriteEndElement(); //cac:TaxSubtotal
				W.WriteEndElement(); //cac:TaxTotal

				W.WriteStartElement("cac:LegalMonetaryTotal"); //Totales a pagar de la Factura y Cargos

				
				W.WriteStartElement("cbc:LineExtensionAmount"); //<cbc:LineExtensionAmount>
				W.WriteStartAttribute("currencyID");
				W.WriteValue(MONEDA);
				W.WriteEndAttribute();

				if (oTransaccion.nTraEsGratuito == Enumerador.ESTADO_ACTIVO)
					W.WriteValue("0.00");
				else
					W.WriteValue(String.Format(CultureInfo.InvariantCulture,
						"{0:0.00}", (oTransaccion.LstTransaccionDetalle.Sum(x => x.nTDeBase))));

				W.WriteEndElement(); //cbc:LineExtensionAmount

				//--
				W.WriteStartElement("cbc:TaxInclusiveAmount"); //<cbc:LineExtensionAmount>
				W.WriteStartAttribute("currencyID");
				W.WriteValue(MONEDA);
				W.WriteEndAttribute();
				W.WriteValue(String.Format(CultureInfo.InvariantCulture, "{0:0.00}", oTransaccion.nTraImporteTotal));
				W.WriteEndElement(); //cbc:LineExtensionAmount

				W.WriteStartElement("cbc:PayableAmount");
				W.WriteStartAttribute("currencyID");
				W.WriteValue(MONEDA);
				W.WriteEndAttribute();

				if (oTransaccion.nTraEsGratuito == Enumerador.ESTADO_ACTIVO)
					W.WriteValue("0.00");
				else
					W.WriteValue(String.Format(CultureInfo.InvariantCulture, "{0:0.00}", oTransaccion.nTraImporteTotal));

				W.WriteEndElement(); //cbc:PayableAmount
				W.WriteEndElement(); //cac:LegalMonetaryTotal

				GenerarDetalleXml(oTransaccion, W, oEmpresa);

				// ***** Copiando el XML *****
				W.WriteEndElement(); // Cerrando Nodos Principales
									 // ***** Grabando datos del XML *****
				W.Flush();
				W.Close();

				#endregion

				#region  INCRUSTAR CERTIFICADO

				Certificado(strNombreArchivo, oTransaccion.TipoDocumento_Id, p_oEmpresa);

				#endregion

				#region COMPRIMIR EL ARCHIVO XML FORMADO A ZIP

				ComprimirXML(strNombreArchivo, oTransaccion.TipoDocumento_Id, p_oEmpresa);

				#endregion


				return true;
			}
			catch (Exception ex)
			{
				LogApplicationNeg.Instance.GuardarLogAplicacion(ex);
				return false;
			}
		}




	}
}
