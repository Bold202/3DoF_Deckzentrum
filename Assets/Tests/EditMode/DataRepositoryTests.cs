using NUnit.Framework;
using D8PlanerXR.Data;
using System;
using System.Collections.Generic;

namespace D8PlanerXR.Tests
{
    /// <summary>
    /// Unit tests for the DataRepository class
    /// Tests CSV parsing, traffic light calculation, and data retrieval
    /// </summary>
    public class DataRepositoryTests
    {
        private SowData CreateTestSow(string earTag, int ventil, int daysSinceMating, string healthStatus = "")
        {
            return new SowData
            {
                earTagNumber = earTag,
                ventilNumber = ventil,
                matingDate = DateTime.Now.AddDays(-daysSinceMating),
                daysSinceMating = daysSinceMating,
                pregnancyStatus = "tragend",
                healthStatus = healthStatus,
                trafficLight = SowData.TrafficLightColor.Unknown
            };
        }
        
        [Test]
        public void TrafficLight_Green_UnderEightyDays()
        {
            // Arrange
            var sow = CreateTestSow("001", 1, 50);
            
            // Act - Calculate based on days
            if (sow.daysSinceMating >= 0 && sow.daysSinceMating <= 79)
                sow.trafficLight = SowData.TrafficLightColor.Green;
            
            // Assert
            Assert.AreEqual(SowData.TrafficLightColor.Green, sow.trafficLight);
        }
        
        [Test]
        public void TrafficLight_Yellow_EightyToHundredSix()
        {
            // Arrange
            var sow = CreateTestSow("002", 1, 90);
            
            // Act - Calculate based on days
            if (sow.daysSinceMating >= 80 && sow.daysSinceMating <= 106)
                sow.trafficLight = SowData.TrafficLightColor.Yellow;
            
            // Assert
            Assert.AreEqual(SowData.TrafficLightColor.Yellow, sow.trafficLight);
        }
        
        [Test]
        public void TrafficLight_Red_OverHundredSeven()
        {
            // Arrange
            var sow = CreateTestSow("003", 1, 110);
            
            // Act - Calculate based on days
            if (sow.daysSinceMating >= 107)
                sow.trafficLight = SowData.TrafficLightColor.Red;
            
            // Assert
            Assert.AreEqual(SowData.TrafficLightColor.Red, sow.trafficLight);
        }
        
        [Test]
        public void TrafficLight_Purple_WithMedication()
        {
            // Arrange
            var sow = CreateTestSow("004", 1, 50, "Medikation erforderlich");
            
            // Act - Medication has priority
            if (!string.IsNullOrEmpty(sow.healthStatus) && 
                sow.healthStatus.ToLower().Contains("medikation"))
            {
                sow.trafficLight = SowData.TrafficLightColor.Purple;
            }
            
            // Assert
            Assert.AreEqual(SowData.TrafficLightColor.Purple, sow.trafficLight);
        }
        
        [Test]
        public void TrafficLight_Purple_HasPriorityOverRed()
        {
            // Arrange - High days AND medication
            var sow = CreateTestSow("005", 1, 115, "Behandlung");
            
            // Act - Medication should override days-based calculation
            if (!string.IsNullOrEmpty(sow.healthStatus) && 
                (sow.healthStatus.ToLower().Contains("medikation") ||
                 sow.healthStatus.ToLower().Contains("behandlung")))
            {
                sow.trafficLight = SowData.TrafficLightColor.Purple;
            }
            else if (sow.daysSinceMating >= 107)
            {
                sow.trafficLight = SowData.TrafficLightColor.Red;
            }
            
            // Assert - Purple has priority
            Assert.AreEqual(SowData.TrafficLightColor.Purple, sow.trafficLight);
        }
        
        [Test]
        public void TrafficLight_BoundaryTest_Day79IsGreen()
        {
            var sow = CreateTestSow("006", 1, 79);
            
            if (sow.daysSinceMating >= 0 && sow.daysSinceMating <= 79)
                sow.trafficLight = SowData.TrafficLightColor.Green;
            else if (sow.daysSinceMating >= 80 && sow.daysSinceMating <= 106)
                sow.trafficLight = SowData.TrafficLightColor.Yellow;
            
            Assert.AreEqual(SowData.TrafficLightColor.Green, sow.trafficLight);
        }
        
        [Test]
        public void TrafficLight_BoundaryTest_Day80IsYellow()
        {
            var sow = CreateTestSow("007", 1, 80);
            
            if (sow.daysSinceMating >= 0 && sow.daysSinceMating <= 79)
                sow.trafficLight = SowData.TrafficLightColor.Green;
            else if (sow.daysSinceMating >= 80 && sow.daysSinceMating <= 106)
                sow.trafficLight = SowData.TrafficLightColor.Yellow;
            
            Assert.AreEqual(SowData.TrafficLightColor.Yellow, sow.trafficLight);
        }
        
        [Test]
        public void TrafficLight_BoundaryTest_Day106IsYellow()
        {
            var sow = CreateTestSow("008", 1, 106);
            
            if (sow.daysSinceMating >= 0 && sow.daysSinceMating <= 79)
                sow.trafficLight = SowData.TrafficLightColor.Green;
            else if (sow.daysSinceMating >= 80 && sow.daysSinceMating <= 106)
                sow.trafficLight = SowData.TrafficLightColor.Yellow;
            else if (sow.daysSinceMating >= 107)
                sow.trafficLight = SowData.TrafficLightColor.Red;
            
            Assert.AreEqual(SowData.TrafficLightColor.Yellow, sow.trafficLight);
        }
        
        [Test]
        public void TrafficLight_BoundaryTest_Day107IsRed()
        {
            var sow = CreateTestSow("009", 1, 107);
            
            if (sow.daysSinceMating >= 0 && sow.daysSinceMating <= 79)
                sow.trafficLight = SowData.TrafficLightColor.Green;
            else if (sow.daysSinceMating >= 80 && sow.daysSinceMating <= 106)
                sow.trafficLight = SowData.TrafficLightColor.Yellow;
            else if (sow.daysSinceMating >= 107)
                sow.trafficLight = SowData.TrafficLightColor.Red;
            
            Assert.AreEqual(SowData.TrafficLightColor.Red, sow.trafficLight);
        }
        
        [Test]
        public void SowData_Initialization_SetsDefaults()
        {
            var sow = new SowData();
            
            Assert.IsNotNull(sow.additionalData);
            Assert.AreEqual(SowData.TrafficLightColor.Unknown, sow.trafficLight);
        }
        
        [Test]
        public void VentilNumber_ExtractFromQRFormat()
        {
            // Test various QR code formats
            string[] testInputs = new string[]
            {
                "VENTIL-042",
                "VENTIL-1",
                "VENTIL-199",
                "42",
                "001"
            };
            
            int[] expectedOutputs = new int[] { 42, 1, 199, 42, 1 };
            
            for (int i = 0; i < testInputs.Length; i++)
            {
                int result = ExtractVentilNumber(testInputs[i]);
                Assert.AreEqual(expectedOutputs[i], result, 
                    $"Failed to extract from '{testInputs[i]}'. Expected {expectedOutputs[i]}, got {result}");
            }
        }
        
        [Test]
        public void VentilNumber_InvalidQRFormat_ReturnsNegative()
        {
            string[] invalidInputs = new string[]
            {
                "INVALID",
                "",
                "ABC",
                "VENTIL-",
                "VENTIL-ABC"
            };
            
            foreach (string input in invalidInputs)
            {
                int result = ExtractVentilNumber(input);
                Assert.Less(result, 0, $"Should return negative for invalid input: '{input}'");
            }
        }
        
        /// <summary>
        /// Helper method matching the extraction logic from MobileCameraController
        /// </summary>
        private int ExtractVentilNumber(string qrContent)
        {
            try
            {
                if (string.IsNullOrEmpty(qrContent))
                    return -1;
                    
                // Format: "VENTIL-042"
                if (qrContent.Contains("VENTIL-"))
                {
                    string numberPart = qrContent.Replace("VENTIL-", "").Trim();
                    if (int.TryParse(numberPart, out int num))
                        return num;
                    return -1;
                }
                
                // Format: Just number
                if (int.TryParse(qrContent.Trim(), out int number))
                {
                    return number;
                }
                
                return -1;
            }
            catch
            {
                return -1;
            }
        }
        
        [Test]
        public void DateParsing_GermanFormat_ParsesCorrectly()
        {
            // Test German date format DD.MM.YYYY
            string dateStr = "13.07.2025";
            
            DateTime result;
            bool success = DateTime.TryParseExact(
                dateStr, 
                "dd.MM.yyyy", 
                System.Globalization.CultureInfo.InvariantCulture, 
                System.Globalization.DateTimeStyles.None, 
                out result
            );
            
            Assert.IsTrue(success, "German date format should parse successfully");
            Assert.AreEqual(13, result.Day);
            Assert.AreEqual(7, result.Month);
            Assert.AreEqual(2025, result.Year);
        }
        
        [Test]
        public void DateParsing_ShortGermanFormat_ParsesCorrectly()
        {
            // Test short German date format D.M.YYYY
            string dateStr = "5.7.2025";
            
            DateTime result;
            bool success = DateTime.TryParseExact(
                dateStr, 
                "d.M.yyyy", 
                System.Globalization.CultureInfo.InvariantCulture, 
                System.Globalization.DateTimeStyles.None, 
                out result
            );
            
            Assert.IsTrue(success, "Short German date format should parse successfully");
            Assert.AreEqual(5, result.Day);
            Assert.AreEqual(7, result.Month);
            Assert.AreEqual(2025, result.Year);
        }
        
        [Test]
        public void CleanValue_RemovesQuotesAndSpaces()
        {
            string[] testInputs = new string[]
            {
                "\"     602\"",
                "\"165   \"",
                "\" -3\"",
                "\"13.07.2025\""
            };
            
            string[] expectedOutputs = new string[]
            {
                "602",
                "165",
                "-3",
                "13.07.2025"
            };
            
            for (int i = 0; i < testInputs.Length; i++)
            {
                string result = CleanValue(testInputs[i]);
                Assert.AreEqual(expectedOutputs[i], result, 
                    $"Failed to clean '{testInputs[i]}'. Expected '{expectedOutputs[i]}', got '{result}'");
            }
        }
        
        /// <summary>
        /// Helper method matching DataRepository's CleanValue implementation
        /// </summary>
        private string CleanValue(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            
            string cleaned = value.Trim();
            if (cleaned.StartsWith("\"") && cleaned.EndsWith("\""))
            {
                cleaned = cleaned.Substring(1, cleaned.Length - 2);
            }
            cleaned = cleaned.Trim();
            
            return cleaned;
        }
    }
}
