using System.Globalization;
using System.Windows.Forms;

namespace UI
{
    public static class FormatoPresentacion
    {
        public static readonly CultureInfo Cultura = CultureInfo.GetCultureInfo("es-AR");

        public static string Moneda(decimal importe) { return importe.ToString("C2", Cultura); }

        public static void Aplicar(Control control)
        {
            var fecha = control as DateTimePicker;
            if (fecha != null && (fecha.Format == DateTimePickerFormat.Short || fecha.Format == DateTimePickerFormat.Long)) {
                fecha.Format = DateTimePickerFormat.Custom; fecha.CustomFormat = "dd/MM/yyyy";
            }
            var tabla = control as DataGridView;
            if (tabla != null) {
                tabla.DefaultCellStyle.FormatProvider = Cultura;
                tabla.AlternatingRowsDefaultCellStyle.FormatProvider = Cultura;
            }
        }
    }
}
