﻿using System;
using System.Threading.Tasks;
using System.Web.OData;
using System.Web.OData.Query;
using AutoMapper;
using Hach.Fusion.Core.Business.Facades;
using Hach.Fusion.Core.Business.Results;
using Hach.Fusion.Core.Business.Validation;
using Hach.Fusion.FFCO.Business.Database;
using Hach.Fusion.FFCO.Dtos.Dashboards;

namespace Hach.Fusion.FFCO.Business.Facades
{
    /// <summary>
    /// Facade for managing the Dashboard repository. 
    /// </summary>    
    public class DashboardFacade
        : FacadeWithCruModelsBase<DashboardCommandDto, DashboardCommandDto, DashboardQueryDto, Guid>
    {
        private readonly DataContext _context;

        private readonly IMapper _mapper;

        /// <summary>
        /// Constructor for the <see cref="DashboardFacade"/> class taking a database context
        /// and validator argument.
        /// </summary>
        /// <param name="context">Database context containing dashboard type entities.</param>
        /// <param name="validator">Validator for DTOs.</param>
        public DashboardFacade(DataContext context, IFFValidator<DashboardCommandDto> validator)
        {
            _context = context;

            ValidatorCreate = validator;
            ValidatorUpdate = validator;

            _mapper = MappingManager.AutoMapper;
        }

        #region Get Methods

        /// <summary>
        /// Gets a list of dashboards from the data store.
        /// </summary>
        /// <param name="queryOptions">OData query options.</param>
        /// <returns>
        /// An asynchronous task result containing information needed to create an API response message.
        /// If successful, the task result contains the list of DTOs retrieved.
        /// </returns>
        public override async Task<QueryResult<DashboardQueryDto>> Get(ODataQueryOptions<DashboardQueryDto> queryOptions)
        {
            queryOptions.Validate(ValidationSettings);

            throw new NotImplementedException();

            // TODO: tenant checking
        }

        /// <summary>
        /// Gets a single dashboard from the data store.
        /// </summary>
        /// <param name="id">ID that uniquely identifies the entity to be retrieved.</param>
        /// <returns>
        /// An asynchronous task result containing information needed to create an API response message.
        /// If successful, the task result includes the DTO retrieved.
        /// </returns>
        public override async Task<QueryResult<DashboardQueryDto>> Get(Guid id)
        {
            throw new NotImplementedException();

            // TODO: tenant checking
        }

        #endregion Get Methods

        #region Create Method

        /// <summary>
        /// Creates a dashboard.
        /// </summary>
        /// <param name="dto">Data Transfer Object (DTO) used to create an entity.</param>
        /// <returns>
        /// An asynchronous task result containing information needed to create an API response message.
        /// If successful, the task result contains the DTO associated with the entity created.
        /// </returns>
        public override async Task<CommandResult<DashboardQueryDto, Guid>> Create(DashboardCommandDto dto)
        {
            throw new NotImplementedException();

            // TODO: tenant checking
        }

        #endregion Create Method

        #region Delete Method

        /// <summary>
        /// Deletes a dashboard.
        /// </summary>
        /// <param name="id">ID that identifies the entity to be deleted.</param>
        /// <returns>
        /// An asynchronous task result containing information needed to create an API response message.
        /// </returns>
        public override async Task<CommandResult<DashboardQueryDto, Guid>> Delete(Guid id)
        {
            throw new NotImplementedException();

            // TODO: tenant checking
        }

        #endregion Delete Method

        #region Update Method

        /// <summary>
        /// Updates a dashboard using a <see cref="Delta"/> object.
        /// </summary>
        /// <param name="id">ID of the entity to be updated.</param>
        /// <param name="delta">
        /// Delta containing a list of entity properties.  Web Api does the magic of converting the JSON to 
        /// a delta.
        /// </param>
        /// <returns>
        /// An asynchronous task result containing information needed to create an API response message.
        /// </returns>
        public override async Task<CommandResult<DashboardCommandDto, Guid>> Update(Guid id, Delta<DashboardCommandDto> delta)
        {
            throw new NotImplementedException();

            // TODO: tenant checking
        }

        #endregion Update Method

        #region Not Implemented Methods

        public override Task<QueryResult<DashboardQueryDto>> GetProperty(Guid id, string propertyName)
        {
            throw new NotImplementedException();
        }

        public override Task<CommandResult<DashboardQueryDto, Guid>> CreateReference(Guid id, string navigationProperty, object referenceId)
        {
            throw new NotImplementedException();
        }

        public override Task<CommandResult<DashboardQueryDto, Guid>> DeleteReference(Guid id, string navigationProperty, object referenceId)
        {
            throw new NotImplementedException();
        }

        #endregion Not Implemented Methods
    }
}
