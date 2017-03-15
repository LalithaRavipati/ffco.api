using System;
using System.Data.Entity;
using System.Threading.Tasks;
using Hach.Fusion.Core.Business.Validation;
using Hach.Fusion.Data.Entities;
using Hach.Fusion.Data.Dtos;
using Hach.Fusion.Data.Spatial;

namespace Hach.Fusion.FFCO.Business.Validators
{
    /// <summary>
    /// Validates <see cref="LocationBaseDto"/>s.
    /// </summary>
    /// <remarks>
    /// In addition to the override of the <see cref="Validate"/> method, this class contains a static method
    /// used to test whether updating the parent of a location would result in a circular reference.
    /// </remarks>
    public class LocationValidator : FFValidator<LocationBaseDto, Guid>
    {
        private const int NameMaximumLength = 200;

        private const int NameMinimumLength = 4;

        /// <summary>
        /// Validates the state of the specified <see cref="LocationBaseDto"/>.
        /// </summary>
        /// <param name="dto">Data transfer object whose state is to be validated.</param>
        public override FFValidationResponse Validate(LocationBaseDto dto)
        {
            IsNull(dto);

            if (FFErrors.Count > 0)
                return new FFValidationResponse
                {
                    FFErrors = FFErrors
                };

            Evaluate(l => l.Name, dto.Name)
                .Required()
                .MaxLength(NameMaximumLength)
                .MinLength(NameMinimumLength);

            Evaluate(l => l.LocationTypeId, dto.LocationTypeId)
                .Required();

            IsPointValid(dto.Point);

            return new FFValidationResponse
            {
                FFErrors = FFErrors
            };
        }

        /// <summary>
        /// Add a validation error if the point is not valid.
        /// </summary>
        /// <param name="point">The point to validate.</param>
        private void IsPointValid(Point point)
        {
            if (point == null || !point.IsPointValid())
                FFErrors.Add(ValidationErrorCode.PropertyIsInvalid(nameof(point)));
        }

        /// <summary>
        /// Returns whether an updated location would result in a circular reference.
        /// </summary>
        /// <param name="locations">Database set of locations.</param>
        /// <param name="locationToUpdateDto">
        /// Data transfer object for the location to be updated. The <see cref="LocationBaseDto.ParentId"/> must exist
        /// or an <exception cref="InvalidOperationException"/> will be thrown.
        /// </param>
        /// <param name="id">Id of the location being validated for a circular reference.</param>
        /// <returns>True if the update would result in a circular reference and otherwise false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if either argument is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the location to update is not currently in the repository.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the <see cref="LocationBaseDto.ParentId"/> of the <paramref name="locationToUpdateDto"/> argument or
        /// one of the locations in the <paramref name="locations"/> argument refers to a location that does not exist.
        /// </exception>
        public static async Task<bool> IsCircularReference(IDbSet<Location> locations, LocationBaseDto locationToUpdateDto,
            Guid id)
        {
            if (locations == null)
                throw new ArgumentNullException(nameof(locations));

            if (locationToUpdateDto == null)
                throw new ArgumentNullException(nameof(locationToUpdateDto));

            var locationToUpdate = await locations.FirstOrDefaultAsync(l => l.Id == id);

            if (locationToUpdate == null)
                throw new ArgumentException("Location to update not currently in repository", nameof(locationToUpdateDto));

            var parentNode = locationToUpdateDto.ParentId;

            while (parentNode.HasValue)
            {
                if (parentNode.Value == id)
                    return true;

                // ReSharper disable once AccessToModifiedClosure OK because lambda is execute within loop
                var location = await locations.FirstOrDefaultAsync(l => l.Id == parentNode.Value);

                if (location == null)
                    throw new InvalidOperationException($"ParentId {parentNode.Value} does not exist");

                parentNode = location.ParentId;
            }

            return false;
        }
    }
}
