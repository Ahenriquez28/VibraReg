import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import './Admin.css';
import type { Team, Student, TeamChange } from '../models/AdminTypes';

// const API_URL = import.meta.env.VITE_API_URL ? `${import.meta.env.VITE_API_URL}/api` : 'http://localhost:5001/api';
const API_URL = import.meta.env.VITE_API_URL;

const MAX_TEAM_SIZE = 4;

function Admin() {
  const navigate = useNavigate();
  const username = localStorage.getItem('username');
  const [teams, setTeams] = useState<Team[]>([]);
  const [mode, setMode] = useState<'view' | 'edit'>('view');
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [draggedStudent, setDraggedStudent] = useState<Student & { sourceTeamId: number } | null>(null);
  const [changes, setChanges] = useState<TeamChange[]>([]);
  const [showAddTeam, setShowAddTeam] = useState(false);
  const [newTeamName, setNewTeamName] = useState('');


  useEffect(() => {
  fetchTeams(); // Initial fetch only

  const intervalId = setInterval(() => {
    fetchTeams();
  }, 5000);
    return () => clearInterval(intervalId);
  }, []); 

  const getAuthHeaders = () => {
    const token = localStorage.getItem('authToken');
    return {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    };
  };

  const handleLogout = () => {
    localStorage.clear();
    navigate('/login');
  };

  const fetchTeams = async () => {
    try {
      setLoading(true);
      const token = localStorage.getItem('authToken');
      const response = await fetch(`${API_URL}/getTeams?t=${Date.now()}`, {  // ✅ Add cache buster
        headers: {
          'Authorization': `Bearer ${token}`
        }
      });
      
      if (response.status === 401) {
        handleLogout();
        return;
      }
      
      const data = await response.json();
      if (data.success) setTeams(data.teams);
    } catch (error) {
      console.error('Failed to fetch teams:', error);
      alert('Failed to load teams');
    } finally {
      setLoading(false);
    }
  };

  const handleTogglePresent = async (studentId: number, currentStatus: boolean) => {
    try {
      const response = await fetch(`${API_URL}/togglePresent`, {
        method: 'POST',
        headers: getAuthHeaders(),
        body: JSON.stringify({
          studentId: studentId,
          isPresent: !currentStatus
        })
      });

      if (response.status === 401) {
        handleLogout();
        return;
      }

      const data = await response.json();
      if (data.success) {
        // Update local state
        setTeams(teams.map(team => ({
          ...team,
          students: team.students.map(student =>
            student.id === studentId
              ? { ...student, isPresent: !currentStatus }
              : student
          )
        })));
      } else {
        alert('Failed to update attendance: ' + data.message);
      }
    } catch (error) {
      console.error('Failed to toggle attendance:', error);
      alert('Failed to update attendance');
    }
  };

  const handleDragStart = (e: React.DragEvent, student: Student, sourceTeamId: number) => {
    if (mode !== 'edit') return;
    setDraggedStudent({ ...student, sourceTeamId });
    e.dataTransfer.effectAllowed = 'move';
  };

  const handleDrop = (e: React.DragEvent, targetTeamId: number) => {
    e.preventDefault();
    if (mode !== 'edit' || !draggedStudent) return;

    const { sourceTeamId, ...student } = draggedStudent;
    if (sourceTeamId === targetTeamId) {
      setDraggedStudent(null);
      return;
    }

    const targetTeam = teams.find(t => t.teamId === targetTeamId);
    if (targetTeam && targetTeam.teamId !== 404 && targetTeam.students.length >= MAX_TEAM_SIZE) {
      alert(`This team is full! Maximum ${MAX_TEAM_SIZE} students per team.`);
      setDraggedStudent(null);
      return;
    }

    setTeams(teams.map(team => {
      if (team.teamId === sourceTeamId) {
        return { ...team, students: team.students.filter(s => s.id !== student.id) };
      }
      if (team.teamId === targetTeamId) {
        return { ...team, students: [...team.students, student] };
      }
      return team;
    }));

    setChanges(prev => [
      ...prev.filter(c => c.studentId !== student.id),
      { studentId: student.id, teamId: targetTeamId === 404 ? null : targetTeamId }
    ]);

    setDraggedStudent(null);
  };

  const handleDeleteStudent = async (student: Student, teamId: number) => {
    const confirmMessage = `Are you sure you want to permanently delete ${student.fullName} from the system?`;
    
    if (!window.confirm(confirmMessage)) return;

    try {
      const response = await fetch(`${API_URL}/removeStudents`, {
        method: 'POST',
        headers: getAuthHeaders(),
        body: JSON.stringify({
          assignments: [{ studentId: student.id, teamId: null }]
        })
      });

      if (response.status === 401) {
        handleLogout();
        return;
      }

      const data = await response.json();
      if (data.success) {
        alert('Student deleted successfully!');
        await fetchTeams();
      } else {
        alert('Failed to delete student: ' + data.message);
      }
    } catch (error) {
      console.error('Failed to delete student:', error);
      alert('Failed to delete student');
    }
  };

  const handleCreateTeam = async () => {
    if (!newTeamName.trim()) {
      alert('Please enter a team name');
      return;
    }

    try {
      const response = await fetch(`${API_URL}/createTeam`, {
        method: 'POST',
        headers: getAuthHeaders(),
        body: JSON.stringify({ groupName: newTeamName })
      });

      if (response.status === 401) {
        handleLogout();
        return;
      }

      const data = await response.json();
      if (data.success) {
        alert('Team created successfully!');
        setNewTeamName('');
        setShowAddTeam(false);
        await fetchTeams();
      } else {
        alert('Failed to create team: ' + data.message);
      }
    } catch (error) {
      console.error('Failed to create team:', error);
      alert('Failed to create team');
    }
  };

  const handleDeleteTeam = async (team: Team) => {
    const studentCount = team.students.length;
    const confirmMessage = studentCount > 0 
      ? `Are you sure you want to permanently delete "${team.groupName}"? This team has ${studentCount} student${studentCount !== 1 ? 's' : ''} and they will be unassigned.`
      : `Are you sure you want to permanently delete "${team.groupName}"?`;
    
    if (!window.confirm(confirmMessage)) return;

    try {
      const response = await fetch(`${API_URL}/removeTeam`, {
        method: 'POST',
        headers: getAuthHeaders(),
        body: JSON.stringify({
          teamId: team.teamId,
          groupName: team.groupName,
          teamFull: team.teamFull,
          id: "",
          createdAt: new Date().toISOString(),
          students: []
        })
      });

      if (response.status === 401) {
        handleLogout();
        return;
      }

      const data = await response.json();
      if (data.success) {
        alert('Team deleted successfully!');
        await fetchTeams();
      } else {
        alert('Failed to delete team: ' + data.message);
      }
    } catch (error) {
      console.error('Failed to delete team:', error);
      alert('Failed to delete team');
    }
  };

  const handleSave = async () => {
    if (changes.length === 0) {
      alert('No changes to save');
      return;
    }

    try {
      setSaving(true);
      const response = await fetch(`${API_URL}/updateTeams`, {
        method: 'POST',
        headers: getAuthHeaders(),
        body: JSON.stringify({ assignments: changes })
      });

      if (response.status === 401) {
        handleLogout();
        return;
      }

      const data = await response.json();
      if (data.success) {
        alert('Teams updated successfully!');
        setChanges([]);
        setMode('view');
        await fetchTeams();
      } else {
        alert('Failed to save changes: ' + data.message);
      }
    } catch (error) {
      console.error('Failed to save:', error);
      alert('Failed to save changes');
    } finally {
      setSaving(false);
    }
  };

  const handleCancel = () => {
    setMode('view');
    setChanges([]);
    fetchTeams();
  };

  if (loading) {
    return (
      <div className="admin-container">
        <div className="loading">Loading teams...</div>
      </div>
    );
  }

  return (
    <div className="admin-container">
      {/* Header */}
      <header className="admin-header">
        <div>
          <h1>Team Management Admin Panel</h1>
          <span className="logged-in-as">Logged in as: {username}</span>
        </div>
        <div className="admin-controls">
          {mode === 'view' ? (
            <>
              <button className="btn btn-primary" onClick={() => setMode('edit')}>
                Switch to Edit Mode
              </button>
              <button 
                className="btn btn-secondary" 
                onClick={fetchTeams} 
                style={{ marginLeft: '10px' }}
              >
                🔄 Refresh
              </button>
              <button className="btn btn-secondary" onClick={handleLogout} style={{ marginLeft: '10px' }}>
                Logout
              </button>
            </>
          ) : (
            <div className="edit-controls">
              <span className="changes-indicator">
                {changes.length} change{changes.length !== 1 ? 's' : ''}
              </span>
              <button 
                className="btn btn-success"
                onClick={handleSave}
                disabled={saving || changes.length === 0}
              >
                {saving ? 'Saving...' : 'Save Changes'}
              </button>
              <button className="btn btn-secondary" onClick={handleCancel} disabled={saving}>
                Cancel
              </button>
              <button
                className="btn btn-primary"
                onClick={() => setShowAddTeam(true)}
                style={{ marginLeft: '10px' }}
              >
                + Add Team
              </button>
            </div>
          )}
        </div>
      </header>

      {/* Edit instructions */}
      {mode === 'edit' && (
        <div className="edit-instructions">
          <strong>Edit Mode:</strong> Drag and drop students between teams to reorganize. 
          Click Save Changes when done.
        </div>
      )}

      {/* Add Team Modal */}
      {showAddTeam && (
        <div className="add-team-modal">
          <div className="add-team-content">
            <h3>Create New Team</h3>
            <input
              type="text"
              placeholder="Team name"
              value={newTeamName}
              onChange={(e) => setNewTeamName(e.target.value)}
            />
            <div className="modal-actions">
              <button
                className="btn btn-success"
                onClick={handleCreateTeam}
              >
                Create
              </button>
              <button
                className="btn btn-secondary"
                onClick={() => {
                  setShowAddTeam(false);
                  setNewTeamName('');
                }}
              >
                Cancel
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Teams Grid */}
      <div className="teams-grid">
        {teams.map(team => (
          <TeamCard
            key={team.teamId}
            team={team}
            mode={mode}
            onDragStart={handleDragStart}
            onDragOver={(e) => { if (mode === 'edit') e.preventDefault(); }}
            onDrop={handleDrop}
            onDeleteTeam={handleDeleteTeam}
            onDeleteStudent={handleDeleteStudent}
            onTogglePresent={handleTogglePresent}
          />
        ))}
      </div>
    </div>
  );
}

interface TeamCardProps {
  team: Team;
  mode: 'view' | 'edit';
  onDragStart: (e: React.DragEvent, student: Student, teamId: number) => void;
  onDragOver: (e: React.DragEvent) => void;
  onDrop: (e: React.DragEvent, teamId: number) => void;
  onDeleteTeam: (team: Team) => void;
  onDeleteStudent: (student: Student, teamId: number) => void;
  onTogglePresent: (studentId: number, currentStatus: boolean) => void;
}

function TeamCard({ team, mode, onDragStart, onDragOver, onDrop, onDeleteTeam, onDeleteStudent, onTogglePresent }: TeamCardProps) {
  const isUnassigned = team.teamId === 404;

  return (
    <div
      className={`team-card ${team.teamFull ? 'team-full' : ''} ${isUnassigned ? 'unassigned-team' : ''}`}
      onDragOver={onDragOver}
      onDrop={(e) => onDrop(e, team.teamId)}
    >
      <div className="team-header">
        <h2>{team.groupName}</h2>
        <span className="student-count">
          {team.students.length} student{team.students.length !== 1 ? 's' : ''}
          {!isUnassigned && ` / ${MAX_TEAM_SIZE}`}
        </span>
        {team.teamFull && <span className="badge-full">FULL</span>}
        {!isUnassigned && (
          <button
            className="delete-team-btn"
            onClick={() => onDeleteTeam(team)}
            title="Delete team"
          >
            ×
          </button>
        )}
      </div>

      <div className="students-list">
        {team.students.length === 0 ? (
          <div className="empty-team">No students in this team</div>
        ) : (
          team.students.map(student => (
            <StudentCard
              key={student.id}
              student={student}
              teamId={team.teamId}
              mode={mode}
              onDragStart={onDragStart}
              onDeleteStudent={onDeleteStudent}
              onTogglePresent={onTogglePresent}
            />
          ))
        )}
      </div>
    </div>
  );
}

interface StudentCardProps {
  student: Student;
  teamId: number;
  mode: 'view' | 'edit';
  onDragStart: (e: React.DragEvent, student: Student, teamId: number) => void;
  onDeleteStudent: (student: Student, teamId: number) => void;
  onTogglePresent: (studentId: number, currentStatus: boolean) => void;
}

function StudentCard({ student, teamId, mode, onDragStart, onDeleteStudent, onTogglePresent }: StudentCardProps) {
  return (
    <div
      className="student-card"
      draggable={mode === 'edit'}
      onDragStart={(e) => onDragStart(e, student, teamId)}
    >
      <div className="student-info">
        <div className="student-name">{student.fullName}</div>
        <div className="student-email">{student.email}</div>
        <div className="student-details">
          <span>{student.school}</span>
          <span className="gpa">GPA: {student.gpa}</span>
        </div>
        <div className="studentStatus" data-status={student.status}>
          {student.status}
        </div>
        <div>
          {student.resumePath && (
          <a
            href={student.resumePath}
            target="_blank"
            rel="noopener noreferrer"
            className="resume-link"
            onClick={(e) => e.stopPropagation()}
          >
            View Resume
          </a>
        )}
        </div>
        {/* {student.resumePath && (
          <a
            href={student.resumePath}
            target="_blank"
            rel="noopener noreferrer"
            className="resume-link"
            onClick={(e) => e.stopPropagation()}
          >
            View Resume
          </a>
        )} */}
      </div>
      <button
        className={`attendance-btn ${student.isPresent ? 'present' : 'absent'}`}
        onClick={(e) => {
          e.stopPropagation();
          onTogglePresent(student.id, student.isPresent);
        }}
        title={student.isPresent ? 'Mark as absent' : 'Mark as present'}
      >
        <span className="attendance-indicator">●</span>
      </button>
      <button
        className="delete-student-btn"
        onClick={(e) => {
          e.stopPropagation();
          onDeleteStudent(student, teamId);
        }}
        title="Delete student"
      >
        ×
      </button>
      {mode === 'edit' && <div className="drag-indicator">::</div>}
    </div>
  );
}

export default Admin;