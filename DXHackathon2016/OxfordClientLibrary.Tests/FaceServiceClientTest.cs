using System.Threading.Tasks;
using Microsoft.ProjectOxford.Face.Contract;
// <copyright file="FaceServiceClientTest.cs" company="Microsoft">Copyright © 2015 Microsoft</copyright>

using System;
using Microsoft.Pex.Framework;
using Microsoft.Pex.Framework.Validation;
using Microsoft.ProjectOxford.Face;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.ProjectOxford.Face.Tests
{
    [TestClass]
    [PexClass(typeof(FaceServiceClient))]
    [PexAllowedExceptionFromTypeUnderTest(typeof(ArgumentException), AcceptExceptionSubtypes = true)]
    [PexAllowedExceptionFromTypeUnderTest(typeof(InvalidOperationException))]
    public partial class FaceServiceClientTest
    {

        [PexMethod]
        public Task<global::Microsoft.ProjectOxford.Face.Contract.Face[]> DetectAsync(
            [PexAssumeUnderTest]FaceServiceClient target,
            string url,
            bool analyzesFaceLandmarks,
            bool analyzesAge,
            bool analyzesGender,
            bool analyzesHeadPose
        )
        {
            Task<global::Microsoft.ProjectOxford.Face.Contract.Face[]> result
               = target.DetectAsync(url, analyzesFaceLandmarks, analyzesAge, analyzesGender, analyzesHeadPose);
            return result;
            // TODO: add assertions to method FaceServiceClientTest.DetectAsync(FaceServiceClient, String, Boolean, Boolean, Boolean, Boolean)
        }
    }
}
