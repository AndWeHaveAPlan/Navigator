using Microsoft.Extensions.Logging;
using Navigator.Core;
using Navigator.Core.Attributes;
using Navigator.Crow.DataTypes;
using Navigator.Crow.Services;

namespace Navigator.Crow.Controllers
{
    [NavigatorController("Crow")]
    public class ExchangeController
    {
        private readonly StorageService _storageService;
        private readonly ILogger<ExchangeController> _logger;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="storageService"></param>
        public ExchangeController(StorageService storageService, ILogger<ExchangeController> _logger)
        {
            _storageService = storageService;
            this._logger = _logger;
        }

        [NavigatorMethod]
        public GiveResponse Exchange(GiveRequest request)
        {
            _logger.LogInformation(request.ItemCount + " " + request.ItemName);

            return new GiveResponse { ItemCount = 1, ItemName = _storageService.Exchange(request.ItemName) };
        }

        [NavigatorMethod]
        public ActionResult<GiveResponse> ExchangeAction(GiveRequest request)
        {
            _logger.LogInformation(request.ItemCount + " " + request.ItemName);

            return new GiveResponse { ItemCount = 1, ItemName = _storageService.Exchange(request.ItemName) };
        }

        [NavigatorMethod]
        public ActionResult<Empty> Null(GiveRequest request)
        {
            _logger.LogInformation(request.ItemCount + " " + request.ItemName);

            return null;
        }
    }
}
