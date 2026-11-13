
using Microsoft.AspNetCore.Mvc;
using SmartX.Api.Models;
using SmartX.Api.Services;

namespace SmartX.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NetworkTopologyController : ControllerBase
    {
        private readonly NetworkTopologyService _topologyService;

        public NetworkTopologyController(
            NetworkTopologyService topologyService)
        {
            _topologyService = topologyService;
        }

        // GET: api/NetworkTopology
        [HttpGet]
        public ActionResult<NetworkTopologyResponse> GetTopology()
        {
            return Ok(_topologyService.GetTopology());
        }

        // POST: api/NetworkTopology/connections
        [HttpPost("connections")]
        public ActionResult AddConnection(
            [FromBody] NetworkConnectionRequest request)
        {
            try
            {
                _topologyService.AddConnection(
                    request.FirstDevice,
                    request.SecondDevice,
                    request.Weight);

                return Ok(new
                {
                    message = "Network connection created successfully.",
                    firstDevice = request.FirstDevice,
                    secondDevice = request.SecondDevice,
                    weight = request.Weight
                });
            }
            catch (ArgumentException exception)
            {
                return BadRequest(exception.Message);
            }
        }

        // GET: api/NetworkTopology/bfs/{startDevice}
        [HttpGet("bfs/{startDevice}")]
        public ActionResult<List<string>> RunBreadthFirstSearch(
            string startDevice)
        {
            try
            {
                return Ok(
                    _topologyService.RunBreadthFirstSearch(startDevice));
            }
            catch (ArgumentException exception)
            {
                return BadRequest(exception.Message);
            }
        }

        // GET: api/NetworkTopology/dfs/{startDevice}
        [HttpGet("dfs/{startDevice}")]
        public ActionResult<List<string>> RunDepthFirstSearch(
            string startDevice)
        {
            try
            {
                return Ok(
                    _topologyService.RunDepthFirstSearch(startDevice));
            }
            catch (ArgumentException exception)
            {
                return BadRequest(exception.Message);
            }
        }

        // GET: api/NetworkTopology/minimum-spanning-tree
        [HttpGet("minimum-spanning-tree")]
        public ActionResult GetMinimumSpanningTree(
            [FromQuery] string? startDevice = null)
        {
            try
            {
                var connections =
                    _topologyService.GetMinimumSpanningTree(startDevice);

                return Ok(new
                {
                    connections,
                    totalWeight = connections.Sum(edge => edge.Weight),
                    connectionCount = connections.Count
                });
            }
            catch (ArgumentException exception)
            {
                return BadRequest(exception.Message);
            }
        }

        // GET: api/NetworkTopology/shortest-path/{startDevice}/{destinationDevice}
        [HttpGet("shortest-path/{startDevice}/{destinationDevice}")]
        public ActionResult<NetworkPathResult> FindShortestPath(
            string startDevice,
            string destinationDevice)
        {
            try
            {
                var result = _topologyService.FindShortestPath(
                    startDevice,
                    destinationDevice);

                return Ok(result);
            }
            catch (ArgumentException exception)
            {
                return BadRequest(exception.Message);
            }
        }
    }
}
