using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using McbWebApi.Models;
using McbWebApi.Request;
using McbWebApi.Utils;
using System.Web.Http.Cors;

namespace McbWebApi.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [RoutePrefix("api/environmental")]
    public class EnvironmentalController : ApiController
    {
        private McbWebApiContext db = new McbWebApiContext();

        /// <summary>
        /// Return the latest recorded environmental data
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        [ResponseType(typeof(Environmental))]
        public IHttpActionResult GetEnvironmental()
        {
            Environmental environmental = db.Environmentals.OrderByDescending(e => e.id).FirstOrDefault();
            if (environmental == null)
            {
                return NotFound();
            }

            return Ok(environmental);
        }

        /// <summary>
        /// Read the current environmental data from the MCBSTM32F400 board
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("realtime")]
        [ResponseType(typeof(EnvironmentalRT))]
        public IHttpActionResult GetEnvironmentalRT()
        {
            EnvironmentalRT environmentalRT = McbStm32Http.RequestEnvironmentalRT();
            if (environmentalRT == null)
            {
                return NotFound();
            }

            return Ok(environmentalRT);
        }

        /// <summary>
        /// Return historical environmental data
        /// </summary>
        /// <param name="req">Specifies historical data criteria</param>
        /// <returns></returns>
        [HttpPost]
        [Route("history")]
        [ResponseType(typeof(List<Environmental>))]
        public IHttpActionResult PostEnvironmental([FromBody]EnvironmentalRequest req)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            req.End = req.End ?? DateTime.Now;
            req.IntervalMinutes = req.IntervalMinutes ?? 1;

            if(req.Start > req.End)
            {
                return BadRequest("Error: start time must be earlier than end time.");
            }
            if(req.Start > DateTime.Now)
            {
                return BadRequest("Error: time must not be in the future.");
            }
            if(req.Start < DateTime.Now.AddDays(-7))
            {
                return BadRequest("Error: time must be within the past 7 days.");
            }

            string start = req.Start.ToString("yyyy-MM-dd HH:mm:ss");
            string end = ((DateTime)req.End).ToString("yyyy-MM-dd HH:mm:ss");
            int interval = (int)req.IntervalMinutes;
            var query = $@"
                            SELECT ENV.*
                            FROM
                                (SELECT IT.interval, MIN(ID) AS 'first_id'
                                    FROM
                                    (SELECT id, inserted, 
                                        (TIMESTAMPDIFF(MINUTE, '{start}', inserted) DIV {interval}) AS 'interval'
                                        FROM mcbstm32f400.environment
                                        WHERE inserted BETWEEN '{start}' and '{end}') AS IT
                                    GROUP BY IT.interval) AS MI,
                                    mcbstm32f400.environment AS ENV
                            WHERE MI.first_id = ENV.ID

                            UNION

                            SELECT * FROM mcbstm32f400.environment 
                            WHERE inserted BETWEEN ('{end}' + INTERVAL -30 SECOND) AND ('{end}' + INTERVAL 30 SECOND);
                          ";
            var result = db.Environmentals.SqlQuery(query).ToList<Environmental>();
            return Ok(result);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}