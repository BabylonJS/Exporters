using System;
using System.Windows.Forms;

namespace Utilities
{
    public partial class DracoUserControl : UserControl
    {
        public const int quantizePositionBits_default = 14;
        public const int quantizeNormalBits_default = 10;
        public const int quantizeTexcoordBits_default = 12;
        public const int quantizeColorBits_default = 8;
        public const int quantizeGenericBits_default = 12;
        public const bool unifiedQuantization_default = false;

        public DracoUserControl()
        {
            InitializeComponent();
            InitializeValues();
        }

        private void QPositionTrackBar_Scroll(object sender, EventArgs e)
        {
            this.QPositionValueLabel.Text = this.QPositionTrackBar.Value.ToString();
        }
        private void QNormalTrackBar_Scroll(object sender, EventArgs e)
        {
            this.QNormalValueLabel.Text = this.QNormalTrackBar.Value.ToString();
        }
        private void TexcoordTrackBar_Scroll(object sender, EventArgs e)
        {
            this.QTexcoordValueLabel.Text = this.QTexcoordTrackBar.Value.ToString();
        }
        private void QColorTrackBar_Scroll(object sender, EventArgs e)
        {
            this.QColorValueLabel.Text = this.QColorTrackBar.Value.ToString();
        }
        private void QGenericTrackBar_Scroll(object sender, EventArgs e)
        {
            this.QGenericValueLabel.Text = this.QGenericTrackBar.Value.ToString();
        }

        private void ResetButton_Click(object sender, EventArgs e)
        {
            InitializeValues();
        }

        private void InitializeValues()
        {
            this.QPositionTrackBar.Value = quantizePositionBits_default;
            this.QNormalTrackBar.Value = quantizeNormalBits_default;
            this.QTexcoordTrackBar.Value = quantizeTexcoordBits_default;
            this.QColorTrackBar.Value = quantizeColorBits_default;
            this.QGenericTrackBar.Value = quantizeGenericBits_default;
            this.UnifiedCheckBox.Checked = unifiedQuantization_default;

            this.QPositionValueLabel.Text = this.QPositionTrackBar.Value.ToString();
            this.QNormalValueLabel.Text = this.QNormalTrackBar.Value.ToString();
            this.QTexcoordValueLabel.Text = this.QTexcoordTrackBar.Value.ToString();
            this.QColorValueLabel.Text = this.QColorTrackBar.Value.ToString();
            this.QGenericValueLabel.Text = this.QGenericTrackBar.Value.ToString();
        }
    }
}
