import { useState, useEffect } from 'react';
import './Identity.css';

interface User {
  id: number;
  email: string;
  fullName: string;
  role: string;
  status: string;
  createdAt: string;
}

export default function Identity() {
  const [users, setUsers] = useState<User[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchUsers = async () => {
      try {
        // Note: Backend only has getById, so we'll use mock data for list
        // In a real scenario, you'd need to add a GetAll endpoint to the backend
        setUsers([
          { id: 1, email: 'admin@prismerp.com', fullName: 'Admin User', role: 'Admin', status: 'Active', createdAt: '2024-01-15' },
          { id: 2, email: 'manager@prismerp.com', fullName: 'Manager User', role: 'Manager', status: 'Active', createdAt: '2024-02-20' },
          { id: 3, email: 'staff@prismerp.com', fullName: 'Staff User', role: 'Staff', status: 'Active', createdAt: '2024-03-10' },
        ]);
      } catch (error) {
        console.log('API not available, using mock data');
        setUsers([
          { id: 1, email: 'admin@prismerp.com', fullName: 'Admin User', role: 'Admin', status: 'Active', createdAt: '2024-01-15' },
          { id: 2, email: 'manager@prismerp.com', fullName: 'Manager User', role: 'Manager', status: 'Active', createdAt: '2024-02-20' },
          { id: 3, email: 'staff@prismerp.com', fullName: 'Staff User', role: 'Staff', status: 'Active', createdAt: '2024-03-10' },
        ]);
      } finally {
        setLoading(false);
      }
    };

    fetchUsers();
  }, []);

  if (loading) {
    return <div className="loading">Loading...</div>;
  }

  return (
    <div className="module-page">
      <div className="page-header">
        <h1>👥 Identity Module</h1>
        <p>Manage users and access permissions</p>
      </div>

      <div className="page-content">
        <div className="data-table-container">
          <div className="table-header">
            <h2>User List</h2>
            <button className="add-button">+ Add User</button>
          </div>
          
          <table className="data-table">
            <thead>
              <tr>
                <th>ID</th>
                <th>Email</th>
                <th>Full Name</th>
                <th>Role</th>
                <th>Status</th>
                <th>Created At</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {users.map((user) => (
                <tr key={user.id}>
                  <td>{user.id}</td>
                  <td>{user.email}</td>
                  <td>{user.fullName}</td>
                  <td>{user.role}</td>
                  <td>
                    <span className={`status ${user.status.toLowerCase()}`}>
                      {user.status}
                    </span>
                  </td>
                  <td>{user.createdAt}</td>
                  <td>
                    <button className="action-btn edit">Edit</button>
                    <button className="action-btn delete">Delete</button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}
