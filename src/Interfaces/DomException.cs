using System;
using System.Collections.Generic;

namespace AppToolkit.Html.Interfaces
{
    public enum DomExceptionCode
    {
        IndexSizeError = 1,
        HierarchyRequestError = 3,
        WrongDocumentError,
        InvalidCharacterError,
        NoModificationAllowedError = 7,
        NotFoundError,
        NotSupportedError,
        InUseAttributeError,
        InvalidStateError,
        SyntaxError,
        InvalidModificationError,
        NamespaceError,
        InvalidAccessError,
        SecurityError = 18,
        NetworkError,
        AbortError,
        UrlMismatchError,
        QuotaExceededError,
        TimeoutError,
        InvalidNodeTypeError,
        DataCloneError,

        EncodingError = 100,
        NotReadableError,
        UnknownError,
        ConstraintError,
        DataError,
        TransactionInactiveError,
        ReadOnlyError,
        VersionError,
        OperationError
    }

    public class DomException : Exception
    {
        private static readonly Dictionary<DomExceptionCode, string> Descriptions = new Dictionary<DomExceptionCode, string>()
        {
            [DomExceptionCode.IndexSizeError] = "The index is not in the allowed range.",
            [DomExceptionCode.HierarchyRequestError] = "The operation would yield an incorrect node tree.",
            [DomExceptionCode.WrongDocumentError] = "The object is in the wrong document.",
            [DomExceptionCode.InvalidCharacterError] = "The string contains invalid characters.",
            [DomExceptionCode.NoModificationAllowedError] = "The object can not be modified.",
            [DomExceptionCode.NotFoundError] = "The object can not be found here.",
            [DomExceptionCode.NotSupportedError] = "The operation is not supported.",
            [DomExceptionCode.InUseAttributeError] = "The attribute is in use.",
            [DomExceptionCode.InvalidStateError] = "The object is in an invalid state.",
            [DomExceptionCode.SyntaxError] = "The string did not match the expected pattern.",
            [DomExceptionCode.InvalidModificationError] = "The object can not be modified in this way.",
            [DomExceptionCode.NamespaceError] = "The operation is not allowed by Namespaces in XML.",
            [DomExceptionCode.InvalidAccessError] = "The object does not support the operation or argument.",
            [DomExceptionCode.SecurityError] = "The operation is insecure.",
            [DomExceptionCode.NetworkError] = "A network error occurred.",
            [DomExceptionCode.AbortError] = "The operation was aborted.",
            [DomExceptionCode.UrlMismatchError] = "The given URL does not match another URL.",
            [DomExceptionCode.QuotaExceededError] = "The quota has been exceeded.",
            [DomExceptionCode.TimeoutError] = "The operation timed out.",
            [DomExceptionCode.InvalidNodeTypeError] = "The supplied node is incorrect or has an incorrect ancestor for this operation.",
            [DomExceptionCode.DataCloneError] = "The object can not be cloned.",
            [DomExceptionCode.EncodingError] = "The encoding operation (either encoded or decoding) failed.",
            [DomExceptionCode.NotReadableError] = "The I/O read operation failed.",
            [DomExceptionCode.UnknownError] = "The operation failed for an unknown transient reason (e.g. out of memory).",
            [DomExceptionCode.ConstraintError] = "A mutation operation in a transaction failed because a constraint was not satisfied.",
            [DomExceptionCode.DataError] = "Provided data is inadequate.",
            [DomExceptionCode.TransactionInactiveError] = "A request was placed against a transaction which is currently not active, or which is finished.",
            [DomExceptionCode.ReadOnlyError] = "The mutating operation was attempted in a \"readonly\" transaction.",
            [DomExceptionCode.VersionError] = "An attempt was made to open a database using a lower version than the existing version.",
            [DomExceptionCode.OperationError] = "The operation failed for an operation-specific reason."
        };

        public string Name { get; }

        public string Description { get; }

        public DomExceptionCode Code { get; }

        public DomException(DomExceptionCode code)
        {
            Name = code.ToString();
            Description = Descriptions[code];
            Code = code;
        }
    }
}
