using MarsaKasir.Data;
using MarsaKasir.Forms;

Db.Init();
UserRepository.EnsureDefaultAdmin();
ApplicationConfiguration.Initialize();

while (true)
{
    using var login = new LoginForm();
    if (login.ShowDialog() != DialogResult.OK) break;

    var cashier = new CashierForm();
    Application.Run(cashier);
    if (!cashier.LoggedOut) break;   // ditutup lewat tombol X = keluar aplikasi
}