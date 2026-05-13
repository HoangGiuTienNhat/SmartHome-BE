using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmartHome.Application.DTOs.AI;
using SmartHome.Domain.Entities;

namespace SmartHome.Application.Interfaces.Services;

public interface IAiProvider
{
    Task<AiAnalysisResult> AnalyzeCommandAsync(string command, List<Room> rooms, List<OutputDevice> devices);
}
