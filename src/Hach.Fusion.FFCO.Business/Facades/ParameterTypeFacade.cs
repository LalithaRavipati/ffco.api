﻿using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.OData;
using System.Web.OData.Query;
using AutoMapper;
using Hach.Fusion.Core.Business.Facades;
using Hach.Fusion.Core.Business.Results;
using Hach.Fusion.Core.Business.Validation;
using Hach.Fusion.Data.Database;
using Hach.Fusion.Data.Dtos;
using Hach.Fusion.Data.Entities;
using Hach.Fusion.Data.Mapping;


namespace Hach.Fusion.FFCO.Business.Facades
{
    /// <summary>
    /// Facade for managing the ParameterType repository. 
    /// </summary>
    public class ParameterTypeFacade : FacadeBase<ParameterTypeQueryDto, Guid>
    {
        private readonly DataContext _context;

        private readonly IMapper _mapper;

        /// <summary>
        /// Constructor for the <see cref="ParameterTypeFacade"/> class taking a database context argument.
        /// </summary>
        /// <param name="context">Database context containing entities.</param>
        public ParameterTypeFacade(DataContext context)
        {
            _context = context;
            _mapper = MappingManager.AutoMapper;
            // Is this needed? ((IObjectContextAdapter)_context).ObjectContext.ContextOptions.LazyLoadingEnabled = false;
        }

        #region Get Methods

        /// <summary>
        /// Gets a list of ParameterTypes from the data store.
        /// </summary>
        /// <param name="queryOptions">OData query options.</param>
        /// <returns>
        /// An asynchronous task result containing information needed to create an API response message.
        /// If successful, the task result contains the list of DTOs retrieved.
        /// </returns>
        public override async Task<QueryResult<ParameterTypeQueryDto>> Get(ODataQueryOptions<ParameterTypeQueryDto> queryOptions)
        {
            queryOptions.Validate(ValidationSettings);

            var results = await Task.Run(() => _context.ParameterTypes
              .Select(_mapper.Map<ParameterType, ParameterTypeQueryDto>)
              .AsQueryable())
              .ConfigureAwait(false);

            return Query.Result(results);
        }

        /// <summary>
        /// Gets a single parameter type from the data store.
        /// </summary>
        /// <param name="id">ID that uniquely identifies the entity to be retrieved.</param>
        /// <returns>
        /// An asynchronous task result containing information needed to create an API response message.
        /// If successful, the task result includes the DTO retrieved.
        /// </returns>
        public override async Task<QueryResult<ParameterTypeQueryDto>> Get(Guid id)
        {
            var result = await Task.Run(() => _context.ParameterTypes
                .FirstOrDefault(lt => lt.Id == id))
                .ConfigureAwait(false);

            if (result == null)
                return Query.Error(EntityErrorCode.EntityNotFound);

            var dto = _mapper.Map<ParameterType, ParameterTypeQueryDto>(result);

            return Query.Result(dto);
        }

        #endregion Get Methods

        #region Not Implemented Methods

        public override Task<QueryResult<ParameterTypeQueryDto>> GetProperty(Guid id, string propertyName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates an entity (not implemented).
        /// </summary>
        /// <param name="dto">Unused.</param>
        /// <returns>Always throws the Not Implemented Exception.</returns>
        /// <exception cref="NotImplementedException">This exception is always thrown.</exception>
        public override Task<CommandResult<ParameterTypeQueryDto, Guid>> Create(ParameterTypeQueryDto dto)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Updates an entity using a <see cref="Delta"/> object (not implemented).
        /// </summary>
        /// <param name="id">Unused.</param>
        /// <param name="delta">Unused.</param>
        /// <returns>Always throws the Not Implemented Exception.</returns>
        /// <exception cref="NotImplementedException">This exception is always thrown.</exception>
        public override Task<CommandResult<ParameterTypeQueryDto, Guid>> Update(Guid id, Delta<ParameterTypeQueryDto> delta)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Deletes an entity (not implemented).
        /// </summary>
        /// <param name="id">Unused.</param>
        /// <returns>Always throws the Not Implemented Exception.</returns>
        /// <exception cref="NotImplementedException">This exception is always thrown.</exception>
        public override Task<CommandResult<ParameterTypeQueryDto, Guid>> Delete(Guid id)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates a reference to a related entity (not implemented).
        /// </summary>
        /// <param name="id">Unused.</param>
        /// <param name="navigationProperty">Unused.</param>
        /// <param name="referenceId">Unused.</param>
        /// <returns>Always throws the Not Implemented Exception.</returns>
        /// <exception cref="NotImplementedException">This exception is always thrown.</exception>
        public override Task<CommandResult<ParameterTypeQueryDto, Guid>> CreateReference(Guid id, string navigationProperty, object referenceId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Deletes a reference to a related entity (not implemented).
        /// </summary>
        /// <param name="id">Unused.</param>
        /// <param name="navigationProperty">Unused.</param>
        /// <param name="referenceId">Unused.</param>
        /// <returns>Always throws the Not Implemented Exception.</returns>
        /// <exception cref="NotImplementedException">This exception is always thrown.</exception>
        public override Task<CommandResult<ParameterTypeQueryDto, Guid>> DeleteReference(Guid id, string navigationProperty, object referenceId)
        {
            throw new NotImplementedException();
        }

        #endregion Not Implemented Methods
    }
}
