//-----------------------------------------------------------------------------
// <copyright file="ParameterAliasDataSource.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using Microsoft.OData.UriParser;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;

namespace Microsoft.Test.E2E.AspNet.OData.ParameterAlias
{
    #region Define CLR Type
    /// <summary>
    /// entity type
    /// </summary>
    public class Trade
    {
        [Key]
        public int TradeID { get; set; }
        public string ProductName { get; set; }
        public string Description { get; set; }
        public long? TradingVolume { get; set; }
        public CountryOrRegion PortingCountryOrRegion { get; set; }
        public TradeLocation TradeLocation { get; set; }
    }

    /// <summary>
    /// enum type
    /// </summary>
    public enum CountryOrRegion
    {
        Australia,
        USA,
        Canada,
        Italy
    }

    /// <summary>
    /// complex type
    /// </summary>
    public class TradeLocation
    {
        public string City { get; set; }
        public int ZipCode { get; set; }
    }

    #endregion

    #region Define Controller
    public class TradesController : TestODataController
    {
        public TradesController()
        {
            if (null == Trades)
            {
                InitCustomers();
            }
        }

        private static List<Trade> Trades = null;

        private void InitCustomers()
        {
            Trades = new List<Trade>()
                {
                    new Trade()
                        {
                            TradeID = 1,
                            ProductName = "Rice",
                            Description = "Export Rice to USA",
                            PortingCountryOrRegion = CountryOrRegion.USA,
                            TradingVolume = 1000,
                            TradeLocation = new TradeLocation()
                                {
                                    City = "Guangzhou",
                                    ZipCode = 010
                                }
                            
                        },
                    new Trade()
                        {
                            TradeID = 2,
                            ProductName = "Wheat",
                            Description = "Export Wheat to USA",
                            PortingCountryOrRegion = CountryOrRegion.USA,
                            TradingVolume = null,
                            TradeLocation = new TradeLocation()
                                {
                                    City = "Shenzhen",
                                    ZipCode = 100
                                }
                        },
                    new Trade()
                        {
                            TradeID = 3,
                            ProductName = "Wheat",
                            Description = "Export Wheat to Italy",
                            PortingCountryOrRegion = CountryOrRegion.Italy,
                            TradingVolume = 2000,
                            TradeLocation = new TradeLocation()
                                {
                                    City = "Shanghai",
                                    ZipCode = 001
                                }
                        },
                    new Trade()
                        {
                            TradeID = 4,
                            ProductName = "Corn",
                            Description = "Import Corn from USA",
                            PortingCountryOrRegion = CountryOrRegion.USA,
                            TradingVolume = 8000,
                            TradeLocation = new TradeLocation()
                                {
                                    City = "Beijing",
                                    ZipCode = 000
                                }
                        },
                    new Trade()
                        {
                            TradeID = 5,
                            ProductName = "Corn",
                            Description = "Import Corn from Australia",
                            PortingCountryOrRegion = CountryOrRegion.Australia,
                            TradingVolume = 8000,
                            TradeLocation = new TradeLocation()
                                {
                                    City = "Beijing",
                                    ZipCode = 000
                                }
                        },
                    new Trade()
                        {
                            TradeID = 6,
                            ProductName = "Corn",
                            Description = "Import Corn from Canada",
                            PortingCountryOrRegion = CountryOrRegion.Canada,
                            TradingVolume = 6000,
                            TradeLocation = new TradeLocation()
                                {
                                    City = "Beijing",
                                    ZipCode = 000
                                }
                        }
                };
        }

        #region Query
        [EnableQuery]
        public ITestActionResult Get()
        {
            return Ok(Trades.AsQueryable());
        }

        [HttpGet]
        public ITestActionResult HandleUnmappedRequest(ODataPath odataPath)
        {
            var functionSegment = odataPath.Segments.ElementAt(1) as OperationSegment;
            if (functionSegment != null)
            {
                return Ok(functionSegment.GetParameterValue("productName") as string);
            }
            else
            {
                return BadRequest();
            }
        }

        [ODataRoute("Trades/Microsoft.Test.E2E.AspNet.OData.ParameterAlias.GetTradingVolume(productName={productName}, PortingCountryOrRegion={PortingCountryOrRegion})")]
        public ITestActionResult GetTradingVolume([FromODataUri]string productName, CountryOrRegion portingCountryOrRegion)
        {
            var trades = Trades.Where(t => t.ProductName == productName && t.PortingCountryOrRegion == portingCountryOrRegion).ToArray();
            long? tradingVolume = 0;

            foreach (var trade in trades)
            {
                tradingVolume += trade.TradingVolume;
            }
            return Ok(tradingVolume);
        }

        [EnableQuery]
        [ODataRoute("GetTradeByCountry(PortingCountryOrRegion={CountryOrRegion})")]
        public ITestActionResult GetTradeByCountry([FromODataUri] CountryOrRegion countryOrRegion)
        {
            var trades = Trades.Where(t => t.PortingCountryOrRegion == countryOrRegion).ToList();
            return Ok(trades);
        }
        #endregion
    }
    #endregion 
}
