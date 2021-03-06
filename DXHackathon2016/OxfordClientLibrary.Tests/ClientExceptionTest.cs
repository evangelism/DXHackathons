using System.Net;
// <copyright file="ClientExceptionTest.cs" company="Microsoft">Copyright © 2015 Microsoft</copyright>

using System;
using Microsoft.Pex.Framework;
using Microsoft.Pex.Framework.Validation;
using Microsoft.ProjectOxford.Face;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.ProjectOxford.Face.Tests
{
    [TestClass]
    [PexClass(typeof(ClientException))]
    [PexAllowedExceptionFromTypeUnderTest(typeof(ArgumentException), AcceptExceptionSubtypes = true)]
    [PexAllowedExceptionFromTypeUnderTest(typeof(InvalidOperationException))]
    public partial class ClientExceptionTest
    {

        [PexMethod]
        public ClientException BadRequest(string message)
        {
            ClientException result = ClientException.BadRequest(message);
            return result;
            // TODO: add assertions to method ClientExceptionTest.BadRequest(String)
        }

        [PexMethod]
        public ClientException Constructor03(
            string message,
            string errorCode,
            HttpStatusCode httpStatus,
            Exception innerException
        )
        {
            ClientException target = new ClientException(message, errorCode, httpStatus, innerException);
            return target;
            // TODO: add assertions to method ClientExceptionTest.Constructor03(String, String, HttpStatusCode, Exception)
        }
    }
}
