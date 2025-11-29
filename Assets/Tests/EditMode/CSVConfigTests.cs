using NUnit.Framework;
using D8PlanerXR.Data;
using System;

namespace D8PlanerXR.Tests
{
    /// <summary>
    /// Unit tests for CSVColumnConfig and CSV parsing
    /// Tests MusterPlan format compatibility
    /// </summary>
    public class CSVConfigTests
    {
        [Test]
        public void CreateMusterPlanConfig_HasRequiredRoles()
        {
            // Arrange & Act
            var config = CSVColumnConfig.CreateMusterPlanConfig();
            
            // Assert
            Assert.IsNotNull(config.GetColumnByRole(CSVColumnConfig.ColumnRole.VentilNumber), 
                "MusterPlan config must have VentilNumber column");
            Assert.IsNotNull(config.GetColumnByRole(CSVColumnConfig.ColumnRole.EarTagNumber), 
                "MusterPlan config must have EarTagNumber column");
            Assert.IsNotNull(config.GetColumnByRole(CSVColumnConfig.ColumnRole.MatingDate), 
                "MusterPlan config must have MatingDate column");
        }
        
        [Test]
        public void CreateMusterPlanConfig_HasCorrectDelimiter()
        {
            var config = CSVColumnConfig.CreateMusterPlanConfig();
            
            Assert.AreEqual(';', config.delimiter, 
                "MusterPlan uses semicolon delimiter");
        }
        
        [Test]
        public void CreateMusterPlanConfig_HasHeader()
        {
            var config = CSVColumnConfig.CreateMusterPlanConfig();
            
            Assert.IsTrue(config.hasHeader, 
                "MusterPlan has header row");
        }
        
        [Test]
        public void CreateDefaultConfig_IsValid()
        {
            var config = CSVColumnConfig.CreateDefault();
            
            bool isValid = config.Validate(out string errorMessage);
            
            Assert.IsTrue(isValid, $"Default config should be valid. Error: {errorMessage}");
        }
        
        [Test]
        public void CreateMusterPlanConfig_IsValid()
        {
            var config = CSVColumnConfig.CreateMusterPlanConfig();
            
            bool isValid = config.Validate(out string errorMessage);
            
            Assert.IsTrue(isValid, $"MusterPlan config should be valid. Error: {errorMessage}");
        }
        
        [Test]
        public void Config_WithoutVentilColumn_IsInvalid()
        {
            var config = new CSVColumnConfig();
            config.AddColumn("Test", CSVColumnConfig.ColumnType.Text, CSVColumnConfig.ColumnRole.EarTagNumber);
            
            bool isValid = config.Validate(out string errorMessage);
            
            Assert.IsFalse(isValid);
            Assert.IsTrue(errorMessage.Contains("Ventilnummer"));
        }
        
        [Test]
        public void Config_WithoutEarTagColumn_IsInvalid()
        {
            var config = new CSVColumnConfig();
            config.AddColumn("Test", CSVColumnConfig.ColumnType.Number, CSVColumnConfig.ColumnRole.VentilNumber);
            
            bool isValid = config.Validate(out string errorMessage);
            
            Assert.IsFalse(isValid);
            Assert.IsTrue(errorMessage.Contains("Ohrmarkennummer"));
        }
        
        [Test]
        public void AddColumn_SetsCorrectDisplayOrder()
        {
            var config = new CSVColumnConfig();
            
            config.AddColumn("Col1");
            config.AddColumn("Col2");
            config.AddColumn("Col3");
            
            Assert.AreEqual(0, config.columns[0].displayOrder);
            Assert.AreEqual(1, config.columns[1].displayOrder);
            Assert.AreEqual(2, config.columns[2].displayOrder);
        }
        
        [Test]
        public void GetVisibleColumns_FiltersCorrectly()
        {
            var config = new CSVColumnConfig();
            
            var col1 = new CSVColumnConfig.ColumnDefinition("Visible1") { isVisible = true, displayOrder = 0 };
            var col2 = new CSVColumnConfig.ColumnDefinition("Hidden") { isVisible = false, displayOrder = 1 };
            var col3 = new CSVColumnConfig.ColumnDefinition("Visible2") { isVisible = true, displayOrder = 2 };
            
            config.columns.Add(col1);
            config.columns.Add(col2);
            config.columns.Add(col3);
            
            var visible = config.GetVisibleColumns();
            
            Assert.AreEqual(2, visible.Count);
            Assert.AreEqual("Visible1", visible[0].originalName);
            Assert.AreEqual("Visible2", visible[1].originalName);
        }
        
        [Test]
        public void RenameColumn_UpdatesDisplayName()
        {
            var config = CSVColumnConfig.CreateDefault();
            
            bool result = config.RenameColumn("Ventilnummer", "Bucht-Nr.");
            
            Assert.IsTrue(result);
            var col = config.columns.Find(c => c.originalName == "Ventilnummer");
            Assert.AreEqual("Bucht-Nr.", col.displayName);
        }
        
        [Test]
        public void MusterPlanConfig_VentilColumn_AtIndex3()
        {
            // In MusterPlan.csv, ventil number (Sau-Nr.) is at index 3
            var config = CSVColumnConfig.CreateMusterPlanConfig();
            
            var ventilCol = config.GetColumnByRole(CSVColumnConfig.ColumnRole.VentilNumber);
            
            Assert.AreEqual("Sau-Nr.", ventilCol.originalName);
            Assert.AreEqual(3, ventilCol.displayOrder);
        }
        
        [Test]
        public void MusterPlanConfig_EarTagColumn_AtIndex1()
        {
            // In MusterPlan.csv, ear tag (Abf.) is at index 1
            var config = CSVColumnConfig.CreateMusterPlanConfig();
            
            var earTagCol = config.GetColumnByRole(CSVColumnConfig.ColumnRole.EarTagNumber);
            
            Assert.AreEqual("Abf.", earTagCol.originalName);
            Assert.AreEqual(1, earTagCol.displayOrder);
        }
        
        [Test]
        public void MusterPlanConfig_MatingDateColumn_AtIndex4()
        {
            // In MusterPlan.csv, mating date is at index 4 (dynamic date header)
            var config = CSVColumnConfig.CreateMusterPlanConfig();
            
            var matingDateCol = config.GetColumnByRole(CSVColumnConfig.ColumnRole.MatingDate);
            
            Assert.AreEqual("Belegdatum_Spalte5", matingDateCol.originalName);
            Assert.AreEqual(4, matingDateCol.displayOrder);
        }
        
        [Test]
        public void ParseCSVLine_Semicolon_SplitsCorrectly()
        {
            string line = "val1;val2;val3;val4";
            char delimiter = ';';
            
            string[] result = ParseCSVLine(line, delimiter);
            
            Assert.AreEqual(4, result.Length);
            Assert.AreEqual("val1", result[0]);
            Assert.AreEqual("val2", result[1]);
            Assert.AreEqual("val3", result[2]);
            Assert.AreEqual("val4", result[3]);
        }
        
        [Test]
        public void ParseCSVLine_WithQuotes_HandlesCorrectly()
        {
            string line = "\"val;1\";val2;\"val 3\"";
            char delimiter = ';';
            
            string[] result = ParseCSVLine(line, delimiter);
            
            Assert.AreEqual(3, result.Length);
            Assert.AreEqual("val;1", result[0]); // Semicolon preserved inside quotes
            Assert.AreEqual("val2", result[1]);
            Assert.AreEqual("val 3", result[2]);
        }
        
        [Test]
        public void ParseCSVLine_MusterPlanFormat_ParsesCorrectly()
        {
            // Actual line from MusterPlan.csv
            string line = "\" -3\";\"     602\";\"+\";\"165   \";\"13.07.2025\";\" 134\";\"202529\"";
            char delimiter = ';';
            
            string[] result = ParseCSVLine(line, delimiter);
            
            Assert.AreEqual(7, result.Length);
            Assert.AreEqual(" -3", result[0].Trim('"'));
            Assert.AreEqual("     602", result[1].Trim('"'));
            Assert.AreEqual("165   ", result[3].Trim('"'));
        }
        
        /// <summary>
        /// CSV line parser matching DataRepository implementation
        /// </summary>
        private string[] ParseCSVLine(string line, char delimiter)
        {
            System.Collections.Generic.List<string> result = new System.Collections.Generic.List<string>();
            bool inQuotes = false;
            System.Text.StringBuilder currentField = new System.Text.StringBuilder();
            
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == delimiter && !inQuotes)
                {
                    result.Add(currentField.ToString());
                    currentField.Clear();
                }
                else
                {
                    currentField.Append(c);
                }
            }
            
            result.Add(currentField.ToString());
            return result.ToArray();
        }
    }
}
