MySQL.Data - Compiled for .Net Core 2.0 with dataadapter and CommandBuilder Compiled in.

This is essentially the mysql .Net Data adapter downloaded from Oracle as mysql-connector-net-8.0.9-dmr-src.zip

* changed the csproj file to compile with .net core 2.0 ONLY.
* copied src/framework/net452/dataadapter.cs and src/framework/net452/CommandBuilder.cs to src/framework/netstandard
* removed various compiler directives that skipped functions or did a work around for not core 1.6's missing stuff to resolve issues.
* DELETED src/Replication.. I didn't need it and it was the last thing between me and .net core 2.0 with mysqldataadpter bliss.

I am not sure how much time I will have to fix or correct any issues with this but I will do my best. Past the functions I need
I have not tested much. I at the point of inital commit have about 4 hours total in getting this up and running and VERY minimal
testing. However it appears to be working and should serve to allow many to begin porting their code while we wait for Oracle to solve
their rectal-crainal inversion and release a proper package.

Use AT YOUR OWN RISK!
