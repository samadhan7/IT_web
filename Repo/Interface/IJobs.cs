﻿using GTL.Models;

namespace GTL.Repo.Interface
{
	public interface IJobs
	{
		Task<IEnumerable<Job>> GetJobsAsync();
		Task<string> AddJobAsync(Job job);

		Task<bool> DeleteJobAsync(int Id);
		Task<bool> UpdateJobAsync(Job jobData);
	}
}
