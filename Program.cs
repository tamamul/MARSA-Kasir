using MarsaKasir.Data;
using MarsaKasir.Forms;

Db.Init();
ApplicationConfiguration.Initialize();
Application.Run(new CashierForm());   // sebelumnya: new ProductForm()