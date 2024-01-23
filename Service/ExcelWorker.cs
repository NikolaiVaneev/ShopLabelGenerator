using ExcelDataReader;
using ShopLabelGenerator.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ShopLabelGenerator.Service
{
    public static class ExcelWorker
    {
        public static ICollection<BarCode> GetBarCodesData(string filePath)
        {
            ICollection<BarCode> barCodes = new List<BarCode>();

            using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    do
                    {
                        while (reader.Read())
                        {
                            reader.GetValue(0);
                        }
                    } while (reader.NextResult());

                    var conf = new ExcelDataSetConfiguration
                    {
                        UseColumnDataType = true
                    };

                    // Считываем данные первой таблицы 
                    var dataset = reader.AsDataSet(conf);
                    var dataTable = dataset.Tables[1];
                    // и количество столбцов и колонок.
                    int tableRowsCount = dataTable.Rows.Count;
                    int tableColumnsCount = dataTable.Columns.Count;

                    for (var i = 1; i < tableRowsCount; i++)
                    {

                        barCodes.Add(new BarCode()
                        {
                            BARCode = dataTable.Rows[i][0].ToString(),
                            VendorCode = dataTable.Rows[i][1].ToString(),
                            Size = new string(dataTable.Rows[i][3].ToString().Where(char.IsDigit).ToArray()),
                            Count = Convert.ToInt32(dataTable.Rows[i][4])
                        });
                    }
                }
            }
            return barCodes;
        }

        public static List<Product> GetProductsData(string filePath, ICollection<BarCode> barCodes)
        {
            List<Product> products = new List<Product>();

            using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    do
                    {
                        while (reader.Read())
                        {
                            reader.GetValue(0);
                        }
                    } while (reader.NextResult());

                    var conf = new ExcelDataSetConfiguration
                    {
                        UseColumnDataType = true
                    };

                    // Считываем данные первой таблицы 
                    var dataset = reader.AsDataSet(conf);
                    var dataTable = dataset.Tables[0];
                    // и количество столбцов и колонок.
                    int tableRowsCount = dataTable.Rows.Count;
                    int tableColumnsCount = dataTable.Columns.Count;

                    // Извлекаем размеры
                    List<string> _sizes = new List<string>();
                    for (int i = 10; i < tableColumnsCount - 1; i++)
                    {
                        _sizes.Add(dataTable.Rows[0][i].ToString());
                    }

                    // Извлекаем товары
                    for (var i = 1; i < tableRowsCount - 1; i++)
                    {
                        // получаем данные о наличии размеров
                        for (int j = 10; j < tableColumnsCount - 1; j++)
                        {
                            if (string.IsNullOrWhiteSpace(dataTable.Rows[i][j].ToString())){
                                continue;
                            }
                            var sizeCount = int.Parse(dataTable.Rows[i][j].ToString());
                            // если есть, то добавляем
                            if (sizeCount > 0)
                            {
                                var product = new Product
                                {
                                    VendorCode = dataTable.Rows[i][2].ToString(),
                                    Name = dataTable.Rows[i][3].ToString(),
                                    Type = dataTable.Rows[i][4].ToString(),
                                    Color = dataTable.Rows[i][5].ToString(),
                                    TopMaterial = dataTable.Rows[i][6].ToString(),
                                    Insole = dataTable.Rows[i][7].ToString(),
                                    SoleMaterial = dataTable.Rows[i][8].ToString(),
                                    ProductionDate = dataTable.Rows[i][9].ToString(),
                                    Size = _sizes[j - 10],
                                    
                                };
                                // Получаем штрих-код
                                var code = barCodes.Where(u => u.VendorCode == product.VendorCode && u.Size == product.Size).FirstOrDefault();
                                if (code != null)
                                {
                                    product.BARCode = code.BARCode;
                                }
                                

                                for (int k = 0; k < sizeCount; k++)
                                {
                                    products.Add(product);
                                }
                                
                            }
                        }
                    }
                }
            }
            return products;
        }
    }
}
