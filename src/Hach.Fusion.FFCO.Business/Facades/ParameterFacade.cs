using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.OData;
using System.Web.OData.Query;
using AutoMapper;
using Hach.Fusion.Core.Business.Facades;
using Hach.Fusion.Core.Business.Results;
using Hach.Fusion.Core.Business.Validation;
using Hach.Fusion.FFCO.Business.Database;
using Hach.Fusion.FFCO.Dtos;
using Hach.Fusion.FFCO.Entities;

namespace Hach.Fusion.FFCO.Business.Facades
{
    /// <summary>
    /// Facade for managing the parameter repository. 
    /// </summary>
    public class ParameterFacade : FacadeBase<ParameterDto, Guid>
    {
        private readonly DataContext _context;

        private readonly IMapper _mapper;

        /// <summary>
        /// Constructor for the <see cref="ParameterFacade"/> class taking a database context argument.
        /// </summary>
        /// <param name="context">Database context containing entities.</param>
        public ParameterFacade(DataContext context)
        {
            _context = context;
            _mapper = MappingManager.AutoMapper;
            // Is this needed? ((IObjectContextAdapter)_context).ObjectContext.ContextOptions.LazyLoadingEnabled = false;
        }

        #region Get Methods

        /// <summary>
        /// Gets a list of parameters from the data store.
        /// </summary>
        /// <param name="queryOptions">OData query options.</param>
        /// <returns>
        /// An asynchronous task result containing information needed to create an API response message.
        /// If successful, the task result contains the list of DTOs retrieved.
        /// </returns>
        public override async Task<QueryResult<ParameterDto>> Get(ODataQueryOptions<ParameterDto> queryOptions)
        {
            queryOptions.Validate(ValidationSettings);

            var results = await Task.Run(() => _context.Parameters
              .Select(_mapper.Map<Parameter, ParameterDto>)
              .AsQueryable())
              .ConfigureAwait(false);

            return Query.Result(results);
        }

        /// <summary>
        /// Gets a single parameter from the data store.
        /// </summary>
        /// <param name="id">ID that uniquely identifies the entity to be retrieved.</param>
        /// <returns>
        /// An asynchronous task result containing information needed to create an API response message.
        /// If successful, the task result includes the DTO retrieved.
        /// </returns>
        public override async Task<QueryResult<ParameterDto>> Get(Guid id)
        {
            var result = await Task.Run(() => _context.Parameters
                .FirstOrDefault(lt => lt.Id == id))
                .ConfigureAwait(false);

            if (result == null)
                return Query.Error(EntityErrorCode.EntityNotFound);

            var dto = _mapper.Map<Parameter, ParameterDto>(result);

            return Query.Result(dto);
        }

        #endregion Get Methods

        #region Not Implemented Methods

        public override Task<QueryResult<ParameterDto>> GetProperty(Guid id, string propertyName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates an entity (not implemented).
        /// </summary>
        /// <param name="dto">Unused.</param>
        /// <returns>Always throws the Not Implemented Exception.</returns>
        /// <exception cref="NotImplementedException">This exception is always thrown.</exception>
        public override Task<CommandResult<ParameterDto, Guid>> Create(ParameterDto dto)
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
        public override Task<CommandResult<ParameterDto, Guid>> Update(Guid id, Delta<ParameterDto> delta)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Deletes an entity (not implemented).
        /// </summary>
        /// <param name="id">Unused.</param>
        /// <returns>Always throws the Not Implemented Exception.</returns>
        /// <exception cref="NotImplementedException">This exception is always thrown.</exception>
        public override Task<CommandResult<ParameterDto, Guid>> Delete(Guid id)
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
        public override Task<CommandResult<ParameterDto, Guid>> CreateReference(Guid id, string navigationProperty, object referenceId)
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
        public override Task<CommandResult<ParameterDto, Guid>> DeleteReference(Guid id, string navigationProperty, object referenceId)
        {
            throw new NotImplementedException();
        }

        #endregion Not Implemented Methods
    }
}
