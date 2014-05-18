using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ScreenScraper.BulkLoad
{
    public class BaseBulkLoad
    {
        private string[] ColumnNames;

        public BaseBulkLoad(string[] columnNames)
        {
            ColumnNames = columnNames;
        }

        public DataTable ConfigureDataTable()
        {
            var dt = new DataTable();

            for (int i = 0; i < ColumnNames.Length; i++)
            {
                dt.Columns.Add(new DataColumn());
                dt.Columns[i].ColumnName = ColumnNames[i];
            }
            return dt;
        }

        public void BulkCopy<T>(DataTable dt)
        {
            string connString = ConfigurationManager.ConnectionStrings["SymbolContext"].ConnectionString;

            string tableName = typeof(T).Name;

            using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connString))
            {
                for (int i = 0; i < ColumnNames.Length; i++)
                    bulkCopy.ColumnMappings.Add(i, ColumnNames[i]);

                bulkCopy.BulkCopyTimeout = 600; // in seconds 
                bulkCopy.DestinationTableName = tableName;
                try
                {
                    bulkCopy.WriteToServer(dt);
                }
                catch (Exception ex)
                {
                    Log.WriteLog(string.Format("BulkLoadSector - BulkCopy<{0}> - Bulk load error: {1}", tableName, ex.Message));
                }
                bulkCopy.Close();
            }
        }
    }
}
