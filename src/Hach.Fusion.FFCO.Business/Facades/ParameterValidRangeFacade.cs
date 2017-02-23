using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.OData;
using System.Web.OData.Query;
using AutoMapper;
using Hach.Fusion.Core.Business.Facades;
using Hach.Fusion.Core.Business.Results;
using Hach.Fusion.Core.Business.Validation;
using Hach.Fusion.FFCO.Business.Database;
using Hach.Fusion.FFCO.Core.Dtos;
using Hach.Fusion.FFCO.Core.Entities;

namespace Hach.Fusion.FFCO.Business.Facades
{
    /// <summary>
    /// Facade for managing the Parameter Valid Range repository. 
    /// </summary>
    public class ParameterValidRangeFacade
        : FacadeBase<ParameterValidRangeQueryDto, Guid>
    {
        private readonly DataContext _context;

        private readonly IMapper _mapper;

        /// <summary>
        /// Constructor for the <see cref="ParameterValidRangeFacade"/> class taking a database context argument.
        /// </summary>
        /// <param name="context">Database context containing entities.</param>
        public ParameterValidRangeFacade(DataContext context)
        {
            _context = context;
            _context.Configuration.LazyLoadingEnabled = false;
            _mapper = MappingManager.AutoMapper;
        }

        #region Get Methods

        /// <summary>
        /// Gets a list of Parameter Valid Ranges from the data store.
        /// </summary>
        /// <param name="queryOptions">OData query options.</param>
        /// <returns>
        /// An asynchronous task result containing information needed to create an API response message.
        /// If successful, the task result contains the list of DTOs retrieved.
        /// </returns>
        public override async Task<QueryResult<ParameterValidRangeQueryDto>> Get(ODataQueryOptions<ParameterValidRangeQueryDto> queryOptions)
        {
            queryOptions.Validate(ValidationSettings);

            var results = await Task.Run(() => _context.ParameterValidRanges
                .Include(x => x.Parameter)
                .Select(_mapper.Map<ParameterValidRange, ParameterValidRangeQueryDto>)
                .AsQueryable())
                .ConfigureAwait(false);

            return Query.Result(results);
        }

        /// <summary>
        /// Gets a single Parameter Valid Range from the data store.
        /// </summary>
        /// <param name="id">ID that uniquely identifies the entity to be retrieved.</param>
        /// <returns>
        /// An asynchronous task result containing information needed to create an API response message.
        /// If successful, the task result includes the DTO retrieved.
        /// </returns>
        public override async Task<QueryResult<ParameterValidRangeQueryDto>> Get(Guid id)
        {
            var result = await Task.Run(() => _context.ParameterValidRanges
                .Include(x => x.Parameter)
                .FirstOrDefault(r => r.Id == id))
                .ConfigureAwait(false);

            if (result == null)
                return Query.Error(EntityErrorCode.EntityNotFound);

            var dto = _mapper.Map<ParameterValidRange, ParameterValidRangeQueryDto>(result);

            return Query.Result(dto);
        }

        #endregion Get Methods

        #region Not Implemented Methods

        public override Task<QueryResult<ParameterValidRangeQueryDto>> GetProperty(Guid id, string propertyName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates an entity (not implemented).
        /// </summary>
        /// <param name="dto">Unused.</param>
        /// <returns>Always throws the Not Implemented Exception.</returns>
        /// <exception cref="NotImplementedException">This exception is always thrown.</exception>
        public override Task<CommandResult<ParameterValidRangeQueryDto, Guid>> Create(ParameterValidRangeQueryDto dto)
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
        public override Task<CommandResult<ParameterValidRangeQueryDto, Guid>> Update(Guid id, Delta<ParameterValidRangeQueryDto> delta)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Deletes an entity (not implemented).
        /// </summary>
        /// <param name="id">Unused.</param>
        /// <returns>Always throws the Not Implemented Exception.</returns>
        /// <exception cref="NotImplementedException">This exception is always thrown.</exception>
        public override Task<CommandResult<ParameterValidRangeQueryDto, Guid>> Delete(Guid id)
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
        public override Task<CommandResult<ParameterValidRangeQueryDto, Guid>> CreateReference(Guid id, string navigationProperty, object referenceId)
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
        public override Task<CommandResult<ParameterValidRangeQueryDto, Guid>> DeleteReference(Guid id, string navigationProperty, object referenceId)
        {
            throw new NotImplementedException();
        }

        #endregion Not Implemented Methods
    }
}
