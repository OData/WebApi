using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;

namespace WebStack.QA.Test.OData.ParameterAlias
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
        public Country PortingCountry { get; set; }
        public TradeLocation TradeLocation { get; set; }
    }

    /// <summary>
    /// enum type
    /// </summary>
    public enum Country
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
    public class TradesController : ODataController
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
                            PortingCountry = Country.USA,
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
                            PortingCountry = Country.USA,
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
                            PortingCountry = Country.Italy,
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
                            PortingCountry = Country.USA,
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
                            PortingCountry = Country.Australia,
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
                            PortingCountry = Country.Canada,
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
        public IHttpActionResult Get()
        {
            return Ok(Trades.AsQueryable());
        }

        [HttpGet]
        public IHttpActionResult HandleUnmappedRequest(ODataPath path)
        {
            var functionSegment = path.Segments.ElementAt(1) as BoundFunctionPathSegment;
            if (functionSegment != null)
            {
                return Ok(functionSegment.GetParameterValue("productName") as string);
            }
            else
            {
                return BadRequest();
            }
        }

        [ODataRoute("Trades/WebStack.QA.Test.OData.ParameterAlias.GetTradingVolume(productName={productName}, portingCountry={portingCountry})")]
        public IHttpActionResult GetTradingVolume([FromODataUri]string productName, Country portingCountry)
        {
            var trades = Trades.Where(t => t.ProductName == productName && t.PortingCountry == portingCountry).ToArray();
            long? tradingVolume = 0;

            foreach (var trade in trades)
            {
                tradingVolume += trade.TradingVolume;
            }
            return Ok(tradingVolume);
        }

        [EnableQuery]
        [ODataRoute("GetTradeByCountry(portingCountry={country})")]
        public IHttpActionResult GetTradeByCountry([FromODataUri] Country country)
        {
            var trades = Trades.Where(t => t.PortingCountry == country).ToList();
            return Ok(trades);
        }
        #endregion
    }
    #endregion 
}
