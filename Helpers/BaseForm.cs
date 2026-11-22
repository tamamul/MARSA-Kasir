using MarsaKasir.Helpers;
namespace MarsaKasir.Forms;

public class BaseForm : Form
{
    protected override void OnLoad(EventArgs e)
    {
        Theme.Apply(this);
        base.OnLoad(e);
    }
}
