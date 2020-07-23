using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Jarvis.Models;

namespace Jarvis.Services
{
    public interface IFaceRecognitionService
    {
        Task<Models.Face[]> DetectAsync(Stream imageStream, bool returnFaceId, bool returnFaceLandmarks, IEnumerable<FaceAttributeType> returnFaceAttributes);
    }
}
