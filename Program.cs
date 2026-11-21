using MarsaKasir.Data;
Db.Init();    
ApplicationConfiguration.Initialize();
Application.Run(new Form { Text = "MARSA KASIR", WindowState = FormWindowState.Maximized });