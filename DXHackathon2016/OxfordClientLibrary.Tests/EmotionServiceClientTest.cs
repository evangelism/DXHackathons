// <copyright file="EmotionServiceClientTest.cs" company="Microsoft">Copyright © 2015 Microsoft</copyright>

using System;
using Microsoft.Pex.Framework;
using Microsoft.Pex.Framework.Validation;
using Microsoft.ProjectOxford.Emotion;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.ProjectOxford.Emotion.Tests
{
    [TestClass]
    [PexClass(typeof(EmotionServiceClient))]
    [PexAllowedExceptionFromTypeUnderTest(typeof(ArgumentException), AcceptExceptionSubtypes = true)]
    [PexAllowedExceptionFromTypeUnderTest(typeof(InvalidOperationException))]
    public partial class EmotionServiceClientTest
    {

        [PexMethod]
        public EmotionServiceClient Constructor(string subscriptionKey)
        {
            EmotionServiceClient target = new EmotionServiceClient(subscriptionKey);
            return target;
            // TODO: add assertions to method EmotionServiceClientTest.Constructor(String)
        }
    }
}
