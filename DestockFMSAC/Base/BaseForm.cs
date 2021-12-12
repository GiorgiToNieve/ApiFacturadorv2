using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Telerik.WinControls;
using Telerik.WinControls.UI;
using Telerik.WinControls.UI.Export;
using Telerik.WinControls.UI.Localization;
using Utilitarios;

namespace DestockFMSAC.Base
{
    public partial class BaseForm : RadForm
    {

        #region Variables

        private RadForm oForm = new RadForm();
        //private string ThemeName = Enumerador.TEMA_TELERIK_METRO;

        #endregion

        public BaseForm()
        {
            InitializeComponent();

            this.GridViewIdioma_Español();
            ThemeResolutionService.ApplicationThemeName = Enumerador.TEMA_TELERIK_METRO;
        }

        private void BaseForm_Load(object sender, EventArgs e)
        {

        }

        protected void VistaPreviaPDF(string sRuta)
        {
            try
            {
                string sTag = "sRutaPDF=" + sRuta;
                AbrirFormularioDialog<frmVistaPreviaPDF>("", sTag, false);
            }
            catch (Exception)
            {
                throw;
            }
        }


        protected void MostrarMensaje(string sMensaje)
        {
            try
            {
                RadMessageBox.Show(sMensaje, "MENSAJE", MessageBoxButtons.OK, RadMessageIcon.Info);
            }
            catch
            {
                // ignored
            }
        }

        protected DialogResult MostrarPregunta(string sPregunta)
        {
            try
            {
                return RadMessageBox.Show(sPregunta, "CONFIRMAR", MessageBoxButtons.YesNo, RadMessageIcon.Question);
            }
            catch 
            {
                return DialogResult.No;
            }
        }



        protected int ContarCheckBoxGridView(RadGridView GridView)
        {
            try
            {
                int x = 0;
                for (int i = 0; i < GridView.Rows.Count; i++)
                {
                    if (GridView.Rows[i].Cells["dgcOk"].Value != null)
                        if (bool.Parse(GridView.Rows[i].Cells["dgcOk"].Value.ToString()))
                        {
                            x++;
                        }
                }
                return x;
            }
            catch
            {
                return 0;
            }
        }




        protected void GenerarExcelCompleto(string sNombreArchivo, RadGridView GridView)
        {
            try
            {
                string id = Util.FechaFormatoMilitar(DateTime.Now.Date.ToShortDateString()) + DateTime.Now.Minute +
                               "" + DateTime.Now.Millisecond;

                sNombreArchivo = id + "_" + sNombreArchivo;

                string exportFile = Path.GetTempPath() + sNombreArchivo + ".xlsx";
                using (var ms = new MemoryStream())
                {
                    var exporter = new Telerik.WinControls.Export.GridViewSpreadExport(GridView);
                    exporter.ExportChildRowsGrouped = true;
                    exporter.ExportHierarchy = true;
                    exporter.ExportVisualSettings = true;
                    exporter.ChildViewExportMode = ChildViewExportMode.ExportAllViews;
                    exporter.HiddenColumnOption = HiddenOption.DoNotExport;
                    var renderer = new Telerik.WinControls.Export.SpreadExportRenderer();
                    exporter.RunExport(ms, renderer);

                    using (var fileStream = new FileStream(exportFile, FileMode.Create, FileAccess.Write))
                    {
                        ms.WriteTo(fileStream);
                    }

                    System.Diagnostics.Process.Start(exportFile);
                }
            }
            catch
            {
                //    
            }
        }

        protected object AbrirFormularioDialog<T>(string sForTitulo = "", object oTag = null,
           bool bObtenerRespuesta = true)
        {
            try
            {
                object oTagRecuperado = null;

                // Instanciamos formulario
                //var form = (RadForm)Activator.CreateInstance(typeof(T));
                oForm = (BaseForm)Activator.CreateInstance(typeof(T));
                oForm.Text = sForTitulo == string.Empty ? oForm.Text : sForTitulo;
                oForm.Tag = oTag;
                oForm.MaximizeBox = false;
                oForm.StartPosition = FormStartPosition.CenterParent;
                oForm.Activate();

                int DesktopX = (Screen.PrimaryScreen.Bounds.Width - oForm.Size.Width) / 2;
                int DesktopY = (Screen.PrimaryScreen.Bounds.Height - oForm.Size.Height) / 2;

                // 113 mitad del ancho de árbol
                // 21 mitad del alto de panel
                oForm.Location = new Point(DesktopX + 185, DesktopY);

                if (bObtenerRespuesta)
                    oForm.DialogResult = DialogResult.Cancel;

                // Mostramos formulario
                oForm.ShowDialog(this);

                if (bObtenerRespuesta && oForm.DialogResult == DialogResult.OK)
                    oTagRecuperado = oForm.Tag;

                return oTagRecuperado;
            }
            catch (Exception)
            {
                throw;
            }
        }





        #region Idioma Español RadGridView

        #region Clase GridView_Idioma
        public class GridView_Idioma_LocalizationProvider : RadGridLocalizationProvider
        {
            public override string GetLocalizedString(string id)
            {

                #region switch
                switch (id)
                {
                    case RadGridStringId.ConditionalFormattingPleaseSelectValidCellValue: return "Por favor, seleccione valor de celda válida";
                    case RadGridStringId.ConditionalFormattingPleaseSetValidCellValue: return "Por favor, establece un valor de celda válida";
                    case RadGridStringId.ConditionalFormattingPleaseSetValidCellValues: return "Por favor, establece unos valores de celda válida";
                    case RadGridStringId.ConditionalFormattingPleaseSetValidExpression: return "Por favor, establece una expresión válida";
                    case RadGridStringId.ConditionalFormattingItem: return "Item";
                    case RadGridStringId.ConditionalFormattingInvalidParameters: return "Invalid parameters";
                    case RadGridStringId.FilterFunctionBetween: return "Entre";
                    case RadGridStringId.FilterFunctionContains: return "Contiene";
                    case RadGridStringId.FilterFunctionDoesNotContain: return "No Contiene";
                    case RadGridStringId.FilterFunctionEndsWith: return "Termina con";
                    case RadGridStringId.FilterFunctionEqualTo: return "Igual";
                    case RadGridStringId.FilterFunctionGreaterThan: return "Mayor que";
                    case RadGridStringId.FilterFunctionGreaterThanOrEqualTo: return "Mayor o igual a";
                    case RadGridStringId.FilterFunctionIsEmpty: return "Esta vacio";
                    case RadGridStringId.FilterFunctionIsNull: return "Es Nulo";
                    case RadGridStringId.FilterFunctionLessThan: return "Menor que";
                    case RadGridStringId.FilterFunctionLessThanOrEqualTo: return "Menor o igual a";
                    case RadGridStringId.FilterFunctionNoFilter: return "Sin filtro";
                    case RadGridStringId.FilterFunctionNotBetween: return "No esta entre";
                    case RadGridStringId.FilterFunctionNotEqualTo: return "No es igual a";
                    case RadGridStringId.FilterFunctionNotIsEmpty: return "No está vacío";
                    case RadGridStringId.FilterFunctionNotIsNull: return "No es Nulo";
                    case RadGridStringId.FilterFunctionStartsWith: return "Comienza con";
                    case RadGridStringId.FilterFunctionCustom: return "Personalizado";
                    case RadGridStringId.FilterOperatorBetween: return "Entre";
                    case RadGridStringId.FilterOperatorContains: return "Contiene";
                    case RadGridStringId.FilterOperatorDoesNotContain: return "No Contiene";
                    case RadGridStringId.FilterOperatorEndsWith: return "Termina en";
                    case RadGridStringId.FilterOperatorEqualTo: return "Igual";
                    case RadGridStringId.FilterOperatorGreaterThan: return "Mayor a";
                    case RadGridStringId.FilterOperatorGreaterThanOrEqualTo: return "Mayor o igual";
                    case RadGridStringId.FilterOperatorIsEmpty: return "IsEmpty";
                    case RadGridStringId.FilterOperatorIsNull: return "IsNull";
                    case RadGridStringId.FilterOperatorLessThan: return "LessThan";
                    case RadGridStringId.FilterOperatorLessThanOrEqualTo: return "LessThanOrEquals";
                    case RadGridStringId.FilterOperatorNoFilter: return "No filter";
                    case RadGridStringId.FilterOperatorNotBetween: return "NotBetween";
                    case RadGridStringId.FilterOperatorNotEqualTo: return "NotEquals";
                    case RadGridStringId.FilterOperatorNotIsEmpty: return "NotEmpty";
                    case RadGridStringId.FilterOperatorNotIsNull: return "NotNull";
                    case RadGridStringId.FilterOperatorStartsWith: return "Comienza con";
                    case RadGridStringId.FilterOperatorIsLike: return "Parecido";
                    case RadGridStringId.FilterOperatorNotIsLike: return "No se Parece";
                    case RadGridStringId.FilterOperatorIsContainedIn: return "ContainedIn";
                    case RadGridStringId.FilterOperatorNotIsContainedIn: return "NotContainedIn";
                    case RadGridStringId.FilterOperatorCustom: return "Personalizado";
                    case RadGridStringId.CustomFilterMenuItem: return "Personalizado";
                    case RadGridStringId.CustomFilterDialogCaption: return "Mi Filtro [{0}]";
                    case RadGridStringId.CustomFilterDialogLabel: return "Mostrar filas donde:";
                    case RadGridStringId.CustomFilterDialogRbAnd: return "Y";
                    case RadGridStringId.CustomFilterDialogRbOr: return "O";
                    case RadGridStringId.CustomFilterDialogBtnOk: return "OK";
                    case RadGridStringId.CustomFilterDialogBtnCancel: return "Cancelar";
                    case RadGridStringId.CustomFilterDialogCheckBoxNot: return "No";
                    case RadGridStringId.CustomFilterDialogTrue: return "Verdad";
                    case RadGridStringId.CustomFilterDialogFalse: return "Falso";
                    // case RadGridStringId.FilterMenuBlanks: return "Vacío";
                    case RadGridStringId.FilterMenuAvailableFilters: return "Filtros Disponibles";
                    case RadGridStringId.FilterMenuSearchBoxText: return "Buscar...";
                    case RadGridStringId.FilterMenuClearFilters: return "Limpiar Filtro";
                    case RadGridStringId.FilterMenuButtonOK: return "OK";
                    case RadGridStringId.FilterMenuButtonCancel: return "Cancelar";
                    case RadGridStringId.FilterMenuSelectionAll: return "Todo";
                    case RadGridStringId.FilterMenuSelectionAllSearched: return "Buscar Todo";
                    case RadGridStringId.FilterMenuSelectionNull: return "Null";
                    case RadGridStringId.FilterMenuSelectionNotNull: return "Not Null";
                    case RadGridStringId.FilterFunctionSelectedDates: return "Filtrar por fechas específicas:";
                    case RadGridStringId.FilterFunctionToday: return "Hoy";
                    case RadGridStringId.FilterFunctionYesterday: return "Ayer";
                    case RadGridStringId.FilterFunctionDuringLast7days: return "Durante los últimos 7 días";
                    case RadGridStringId.FilterLogicalOperatorAnd: return "AND";
                    case RadGridStringId.FilterLogicalOperatorOr: return "OR";
                    case RadGridStringId.FilterCompositeNotOperator: return "NOT";
                    case RadGridStringId.DeleteRowMenuItem: return "Delete Row";
                    case RadGridStringId.SortAscendingMenuItem: return "Orden ascendente";
                    case RadGridStringId.SortDescendingMenuItem: return "Orden Descendente";
                    case RadGridStringId.ClearSortingMenuItem: return "Limpiar Ordenamiento";
                    case RadGridStringId.ConditionalFormattingMenuItem: return "Formato Condicional";
                    case RadGridStringId.GroupByThisColumnMenuItem: return "Agrupar por esta columna";
                    case RadGridStringId.UngroupThisColumn: return "Desagrupar esta columna";
                    case RadGridStringId.ColumnChooserMenuItem: return "Selector de columna";
                    case RadGridStringId.HideMenuItem: return "Ocultar Columna";
                    case RadGridStringId.HideGroupMenuItem: return "Ocultar Grupo";
                    case RadGridStringId.UnpinMenuItem: return "Desanclar Columna";
                    case RadGridStringId.UnpinRowMenuItem: return "Desanclar Fila";
                    case RadGridStringId.PinMenuItem: return "Estado Pinned";
                    case RadGridStringId.PinAtLeftMenuItem: return "Fijar a la Izquierda";
                    case RadGridStringId.PinAtRightMenuItem: return "Fijar a la Derecha";
                    case RadGridStringId.PinAtBottomMenuItem: return "Fijar en la parte inferior";
                    case RadGridStringId.PinAtTopMenuItem: return "Fijar en la parte Superior";
                    case RadGridStringId.BestFitMenuItem: return "Mejor ajuste";
                    case RadGridStringId.PasteMenuItem: return "Pegar";
                    case RadGridStringId.EditMenuItem: return "Editar";
                    case RadGridStringId.ClearValueMenuItem: return "Limpiar Valor";
                    case RadGridStringId.CopyMenuItem: return "Copiar";
                    case RadGridStringId.CutMenuItem: return "Cortar";
                    case RadGridStringId.AddNewRowString: return "Clic aquí para añadir una nueva fila";
                    case RadGridStringId.SearchRowResultsOfLabel: return "de";
                    case RadGridStringId.SearchRowMatchCase: return "Mayúsculas y Minúsculas";
                    case RadGridStringId.ConditionalFormattingSortAlphabetically: return "Ordenar columnas alfabéticamente";
                    case RadGridStringId.ConditionalFormattingCaption: return "Conditional Formatting Rules Manager";
                    case RadGridStringId.ConditionalFormattingLblColumn: return "Format only cells with";
                    case RadGridStringId.ConditionalFormattingLblName: return "Rule name";
                    case RadGridStringId.ConditionalFormattingLblType: return "Cell value";
                    case RadGridStringId.ConditionalFormattingLblValue1: return "Valor 1";
                    case RadGridStringId.ConditionalFormattingLblValue2: return "Valor 2";
                    case RadGridStringId.ConditionalFormattingGrpConditions: return "Reglas";
                    case RadGridStringId.ConditionalFormattingGrpProperties: return "Propiedades de la regla";
                    case RadGridStringId.ConditionalFormattingChkApplyToRow: return "Apply this formatting to entire row";
                    case RadGridStringId.ConditionalFormattingChkApplyOnSelectedRows: return "Apply this formatting if the row is selected";
                    case RadGridStringId.ConditionalFormattingBtnAdd: return "Add new rule";
                    case RadGridStringId.ConditionalFormattingBtnRemove: return "Eliminar";
                    case RadGridStringId.ConditionalFormattingBtnOK: return "OK";
                    case RadGridStringId.ConditionalFormattingBtnCancel: return "Cancelar";
                    case RadGridStringId.ConditionalFormattingBtnApply: return "Aplicar";
                    case RadGridStringId.ConditionalFormattingRuleAppliesOn: return "Rule applies to";
                    case RadGridStringId.ConditionalFormattingCondition: return "Condition";
                    case RadGridStringId.ConditionalFormattingExpression: return "Expression";
                    case RadGridStringId.ConditionalFormattingChooseOne: return "[Seleccione uno]";
                    case RadGridStringId.ConditionalFormattingEqualsTo: return "equals to [Value1]";
                    case RadGridStringId.ConditionalFormattingIsNotEqualTo: return "is not equal to [Value1]";
                    case RadGridStringId.ConditionalFormattingStartsWith: return "starts with [Value1]";
                    case RadGridStringId.ConditionalFormattingEndsWith: return "ends with [Value1]";
                    case RadGridStringId.ConditionalFormattingContains: return "contains [Value1]";
                    case RadGridStringId.ConditionalFormattingDoesNotContain: return "does not contain [Value1]";
                    case RadGridStringId.ConditionalFormattingIsGreaterThan: return "is greater than [Value1]";
                    case RadGridStringId.ConditionalFormattingIsGreaterThanOrEqual: return "is greater than or equal [Value1]";
                    case RadGridStringId.ConditionalFormattingIsLessThan: return "is less than [Value1]";
                    case RadGridStringId.ConditionalFormattingIsLessThanOrEqual: return "is less than or equal to [Value1]";
                    case RadGridStringId.ConditionalFormattingIsBetween: return "is between [Value1] and [Value2]";
                    case RadGridStringId.ConditionalFormattingIsNotBetween: return "is not between [Value1] and [Value1]";
                    case RadGridStringId.ConditionalFormattingLblFormat: return "Formato";
                    case RadGridStringId.ConditionalFormattingBtnExpression: return "Expression editor";
                    case RadGridStringId.ConditionalFormattingTextBoxExpression: return "Expression";
                    case RadGridStringId.ConditionalFormattingPropertyGridCaseSensitive: return "CaseSensitive";
                    case RadGridStringId.ConditionalFormattingPropertyGridCellBackColor: return "CellBackColor";
                    case RadGridStringId.ConditionalFormattingPropertyGridCellForeColor: return "CellForeColor";
                    case RadGridStringId.ConditionalFormattingPropertyGridEnabled: return "Enabled";
                    case RadGridStringId.ConditionalFormattingPropertyGridRowBackColor: return "RowBackColor";
                    case RadGridStringId.ConditionalFormattingPropertyGridRowForeColor: return "RowForeColor";
                    case RadGridStringId.ConditionalFormattingPropertyGridRowTextAlignment: return "RowTextAlignment";
                    case RadGridStringId.ConditionalFormattingPropertyGridTextAlignment: return "TextAlignment";
                    case RadGridStringId.ConditionalFormattingPropertyGridCaseSensitiveDescription: return "Determines whether case-sensitive comparisons will be made when evaluating string values.";
                    case RadGridStringId.ConditionalFormattingPropertyGridCellBackColorDescription: return "Enter the background color to be used for the cell.";
                    case RadGridStringId.ConditionalFormattingPropertyGridCellForeColorDescription: return "Enter the foreground color to be used for the cell.";
                    case RadGridStringId.ConditionalFormattingPropertyGridEnabledDescription: return "Determines whether the condition is enabled (can be evaluated and applied).";
                    case RadGridStringId.ConditionalFormattingPropertyGridRowBackColorDescription: return "Enter the background color to be used for the entire row.";
                    case RadGridStringId.ConditionalFormattingPropertyGridRowForeColorDescription: return "Enter the foreground color to be used for the entire row.";
                    case RadGridStringId.ConditionalFormattingPropertyGridRowTextAlignmentDescription: return "Enter the alignment to be used for the cell values, when ApplyToRow is true.";
                    case RadGridStringId.ConditionalFormattingPropertyGridTextAlignmentDescription: return "Enter the alignment to be used for the cell values.";
                    case RadGridStringId.ColumnChooserFormCaption: return "Selector de columna";
                    case RadGridStringId.ColumnChooserFormMessage: return "Drag a column header from the\ngrid here to remove it from\nthe current view.";
                    case RadGridStringId.GroupingPanelDefaultMessage: return "Arrastre una Columna para agrupar";
                    case RadGridStringId.GroupingPanelHeader: return "Agrupado por:";
                    case RadGridStringId.PagingPanelPagesLabel: return "Pagina";
                    case RadGridStringId.PagingPanelOfPagesLabel: return "de";
                    case RadGridStringId.NoDataText: return "No existen datos para mostrar";
                    case RadGridStringId.CompositeFilterFormErrorCaption: return "Error al Filtrar";
                    case RadGridStringId.CompositeFilterFormInvalidFilter: return "El descriptor de filtro compuesto no es válido.";
                    case RadGridStringId.ExpressionMenuItem: return "Expression";
                    case RadGridStringId.ExpressionFormTitle: return "Expression Builder";
                    case RadGridStringId.ExpressionFormFunctions: return "Functions";
                    case RadGridStringId.ExpressionFormFunctionsText: return "Text";
                    case RadGridStringId.ExpressionFormFunctionsAggregate: return "Agregado";
                    case RadGridStringId.ExpressionFormFunctionsDateTime: return "Date-Time";
                    case RadGridStringId.ExpressionFormFunctionsLogical: return "Logical";
                    case RadGridStringId.ExpressionFormFunctionsMath: return "Math";
                    case RadGridStringId.ExpressionFormFunctionsOther: return "Otros";
                    case RadGridStringId.ExpressionFormOperators: return "Operadores";
                    case RadGridStringId.ExpressionFormConstants: return "Constantes";
                    case RadGridStringId.ExpressionFormFields: return "Campos";
                    case RadGridStringId.ExpressionFormDescription: return "Descripción";
                    case RadGridStringId.ExpressionFormResultPreview: return "Resultados Previos";
                    case RadGridStringId.ExpressionFormTooltipPlus: return "Más";
                    case RadGridStringId.ExpressionFormTooltipMinus: return "Menos";
                    case RadGridStringId.ExpressionFormTooltipMultiply: return "Multiplica";
                    case RadGridStringId.ExpressionFormTooltipDivide: return "Divide";
                    case RadGridStringId.ExpressionFormTooltipModulo: return "Modulo";
                    case RadGridStringId.ExpressionFormTooltipEqual: return "Igual";
                    case RadGridStringId.ExpressionFormTooltipNotEqual: return "No es Igual";
                    case RadGridStringId.ExpressionFormTooltipLess: return "Menor";
                    case RadGridStringId.ExpressionFormTooltipLessOrEqual: return "Menor o Igual";
                    case RadGridStringId.ExpressionFormTooltipGreaterOrEqual: return "Mayor o Igual";
                    case RadGridStringId.ExpressionFormTooltipGreater: return "Mayor";
                    case RadGridStringId.ExpressionFormTooltipAnd: return "Logical \"AND\"";
                    case RadGridStringId.ExpressionFormTooltipOr: return "Logical \"OR\"";
                    case RadGridStringId.ExpressionFormTooltipNot: return "Logical \"NOT\"";
                    case RadGridStringId.ExpressionFormAndButton: return string.Empty; //if empty, default button image is used
                    case RadGridStringId.ExpressionFormOrButton: return string.Empty; //if empty, default button image is used
                    case RadGridStringId.ExpressionFormNotButton: return string.Empty; //if empty, default button image is used
                    case RadGridStringId.ExpressionFormOKButton: return "OK";
                    case RadGridStringId.ExpressionFormCancelButton: return "Cancelar";
                }
                #endregion
                return string.Empty;
            }
        }
        #endregion


        /// <summary>
        /// [vnieve] Cambia el idioma a Español a los Controles de RadGridview 
        /// Colocar este método en el Constructor del formulario. Ejemplo:
        ///  InitializeComponent();
        /// base.GridViewIdioma_Español(); 
        /// </summary>
        public void GridViewIdioma_Español()
        {

            RadGridLocalizationProvider.CurrentProvider = new GridView_Idioma_LocalizationProvider();

        }


        #endregion
    }
}
