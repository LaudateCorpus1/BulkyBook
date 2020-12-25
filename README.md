# BulkyBook

- Application styling was done by getting the css file from https://bootswatch.com/ website. Select any of the layout you want and replace the code present in wwwroot/lib/bootstrap/dist/css/bootstrap.css. Also make sure you check the _layout file if it is pointing to bootstrap.css or bootstrap.min.css

- If you are getting the below error while migration. Please check the Project in Package console manager.
Your target project 'BulkyBook' doesn't match your migrations assembly 'BulkyBook.DataAccess'. Either change your target project or change your migrations assembly.
Change your migrations assembly by using DbContextOptionsBuilder. E.g. options.UseSqlServer(connection, b => b.MigrationsAssembly("BulkyBook")). By default, the migrations assembly is the assembly containing the DbContext.
Change your target project to the migrations project by using the Package Manager Console's Default project drop-down list, or by executing "dotnet ef" from the directory containing the migrations project.
