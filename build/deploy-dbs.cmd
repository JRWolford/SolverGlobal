docker-compose up -d
docker exec msSql-SolverGlobal /opt/mssql-tools/bin/sqlcmd -U sa -P P@ssW0rd! -Q "RESTORE DATABASE AdventureWorksLT2019 FROM DISK='/var/opt/mssql/backup/AdventureWorksLT2019.bak' WITH MOVE 'AdventureWorksLT2012_data' TO '/var/opt/mssql/data/AdventureWorksLT2019.mdf', MOVE 'AdventureWorksLT2012_log' TO '/var/opt/mssql/data/AdventureWorksLT2019_log.ldf';"