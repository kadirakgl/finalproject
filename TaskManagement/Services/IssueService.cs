using System.Collections.Generic;
using TaskManagement.Models;
using TaskManagement.DataAccess;
using System;

namespace TaskManagement.Services
{
    public class IssueService
    {
        private readonly IssueRepository _issueRepository;
        public IssueService(IssueRepository issueRepository)
        {
            _issueRepository = issueRepository;
        }
        public List<Issue> GetAll() => _issueRepository.GetAll();
        public Issue GetById(int id) => _issueRepository.GetById(id);
        public int AddIssue(Issue issue) => _issueRepository.Add(issue);
        public void UpdateIssue(Issue issue) => _issueRepository.Update(issue);
        public void DeleteIssue(int id) => _issueRepository.Delete(id);
    }
} 