using System;

namespace Product.Dal.Exceptions;

public class EntityValidationError 
    {
        public string ErrorMessage { get; set; }

        public string PropertyName { get; set; }
    }

    public class EntityValidationResult 
    {
        public List<EntityValidationError> ValidationErrors = new List<EntityValidationError>();
    }
   
    public class EntityValidationException : Exception
    {
        public List<EntityValidationResult> EntityValidationErrors = new List<EntityValidationResult>();

        public EntityValidationException()
        {
        }

        public EntityValidationException(string message) : base(message)
        {
        }

        public EntityValidationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
