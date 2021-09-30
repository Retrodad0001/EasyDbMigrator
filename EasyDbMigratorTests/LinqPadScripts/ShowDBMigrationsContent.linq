<Query Kind="Program">
  <Connection>
    <ID>e555e7b1-abf5-404e-8a5e-fd1c8f010de3</ID>
    <NamingServiceVersion>2</NamingServiceVersion>
    <Persist>true</Persist>
    <Driver Assembly="(internal)" PublicKeyToken="no-strong-name">LINQPad.Drivers.EFCore.DynamicDriver</Driver>
    <Server>localhost,1433</Server>
    <UserName>SA</UserName>
    <SqlSecurity>true</SqlSecurity>
    <Password>AQAAANCMnd8BFdERjHoAwE/Cl+sBAAAAtw33oyoHBkeGfbPSZ998iwAAAAACAAAAAAADZgAAwAAAABAAAAC11zJDSo0HFuZ4REJNanI3AAAAAASAAACgAAAAEAAAAIKGs3RkM/ZPNbE5MrF2RGQQAAAAyINgIxzPAuN0dAbDwjVMnhQAAADLxDFMj/HaUAaH2BxxDRnA7FYu6g==</Password>
    <Database>EasyDbMigrator</Database>
    <DriverData>
      <PreserveNumeric1>True</PreserveNumeric1>
      <EFProvider>Microsoft.EntityFrameworkCore.SqlServer</EFProvider>
      <Port>1433</Port>
    </DriverData>
  </Connection>
</Query>

void Main()
{
	var migrationsRun = 
	(
		from p in DbMigrationsRuns
		orderby p.Executed
		select new {Executed = p.Executed.ToString(), Filename = p.Filename}
		
	).ToList();
	
	migrationsRun.Dump();
}

// You can define other methods, fields, classes and namespaces here
