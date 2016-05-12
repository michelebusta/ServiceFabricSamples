using Contracts;
using Shared.Handlers;
using Shared.Helpers;
using Shared.Models;
using Shared.Services;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading.Tasks;
using System.Web.Http;

namespace ApiGateway.Controllers
{
    public class EntitiesController : GenericApiController
    {
        protected const string LOG_TAG = "EntitiesController";

        public EntitiesController() : 
            base()
        {
        }

        [HttpGet]
        [Route("api/entities/all", Name = "AllEntities")]
        public async Task<IHttpActionResult> GetAllEntities()
        {
            var error = "";
            var handler = HandlersFactory.GetProfilerHandler(TheSettingService);
            handler.Start(LOG_TAG, "GetAllEntities", GetServiceProperties());

            try
            {
                IOltpConnectorService connector = ServiceFactory.GetOltpConnectorService(TheSettingService);
                return Ok(await connector.GetEntities());
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return BadRequest(ex.Message);
            }
            finally
            {
                handler.Stop(error);
            }
        }

        [HttpGet]
        [Route("api/entities/{type}/{businesskey}", Name = "EntityView")]
        public async Task<IHttpActionResult> GetEntityView(EntityTypes type, int businesskey)
        {
            var error = "";
            var handler = HandlersFactory.GetProfilerHandler(TheSettingService);
            handler.Start(LOG_TAG, "GetEntityView", GetServiceProperties());

            try
            {
                IActorLocationService actorLocator = ServiceFactory.GetActorLocationService();
                IEntityActor entityActor = actorLocator.Create<IEntityActor>(new Entity(type, businesskey).GetPartitionKey(), Constants.ApplicationName);
                var entity = await entityActor.GetEntity();
                var actorView = await entityActor.GetView();

                var parent = entity.Parent;
                var parentView = new EntityView();
                if (parent != null)
                {
                    IEntityActor parentActor = actorLocator.Create<IEntityActor>(new Entity(parent.Type, parent.BusinessKey).GetPartitionKey(), Constants.ApplicationName);
                    parentView = await parentActor.GetView();
                }

                var children = await entityActor.GetChildren();

                Dictionary<string, EntityView> childrenViews = new Dictionary<string, EntityView>();
                foreach (var child in children)
                {
                    IEntityActor childActor = actorLocator.Create<IEntityActor>(new Entity(child.Type, child.BusinessKey).GetPartitionKey(), Constants.ApplicationName);
                    var childView = await childActor.GetView();
                    childrenViews.Add(child.Name, childView);
                }

                return Ok(new
                {
                    ParentName = parent != null ? parent.Name : "",
                    ParentView = parent != null ? parentView : null,
                    ThisView = actorView,
                    ChildrenViews = childrenViews
                });
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return BadRequest(ex.Message);
            }
            finally
            {
                handler.Stop(error);
            }
        }

        [HttpPost]
        [Route("api/entities/transaction", Name = "EntityTransaction")]
        public async Task<IHttpActionResult> PostReprocess([FromBody] EntityTransaction transaction)
        {
            var error = "";
            var handler = HandlersFactory.GetProfilerHandler(TheSettingService);
            handler.Start(LOG_TAG, "PostReprocess", GetServiceProperties());

            try
            {
                ServiceLocationService locator = new ServiceLocationService();
                UriBuilderService builder = new UriBuilderService(Constants.ApplicationInstance, Constants.EnqueuerServiceName);
                var partitionKey = (long)transaction.EntityType;
                IEnqueuerService enqueuerService = locator.Create<IEnqueuerService>(partitionKey, builder.ToUri());
                bool isEnqueued = await enqueuerService.EnqueueProcess(transaction);
                if (isEnqueued)
                    return Ok();
                else
                    throw new Exception("Enqueuing failed!!!");
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return BadRequest(ex.Message);
            }
            finally
            {
                handler.Stop(error);
            }
        }
    }
}
