using System;
using System.Globalization;
using System.Windows.Forms;

namespace UI
{
    // El editor usa la misma convención que los importes mostrados, sin cambiar
    // la cultura del hilo que también ejecuta reglas e integridad de usuarios.
    public sealed class NumeroRegional : NumericUpDown
    {
        protected override void UpdateEditText()
        {
            if (Hexadecimal) { base.UpdateEditText(); return; }
            if (UserEdit) ValidateEditText();
            ChangingText = true;
            Text = Value.ToString((ThousandsSeparator ? "N" : "F") + DecimalPlaces, FormatoPresentacion.Cultura);
            UserEdit = false;
        }

        protected override void ValidateEditText()
        {
            if (Hexadecimal) { base.ValidateEditText(); return; }
            decimal ingresado;
            bool valido = decimal.TryParse(Text, NumberStyles.Number, FormatoPresentacion.Cultura, out ingresado);
            UserEdit = false;
            if (valido) Value = Math.Min(Maximum, Math.Max(Minimum, ingresado));
            UpdateEditText();
        }

        public override void UpButton() { if (UserEdit) ValidateEditText(); base.UpButton(); }
        public override void DownButton() { if (UserEdit) ValidateEditText(); base.DownButton(); }

        protected override void OnTextBoxKeyPress(object source, KeyPressEventArgs e)
        {
            if (Hexadecimal) { base.OnTextBoxKeyPress(source, e); return; }
            e.Handled = !char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != ',' && e.KeyChar != '.' && e.KeyChar != '-';
            base.OnKeyPress(e);
        }
    }
}
