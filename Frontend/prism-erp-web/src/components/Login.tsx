import { useState } from 'react';
import { useAppDispatch } from '../app/hooks';
import { setAccessToken, setUser, finishHydration } from '../app/userSlice';
import { authApi } from '../services/identityApi';
import { parseApiError, getToastMessage } from '../utils/errorHandler';
import './Login.css';

export default function Login() {
  const dispatch = useAppDispatch();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState('');

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsLoading(true);
    setError('');

    try {
      const data = await authApi.login(email, password);
      dispatch(setAccessToken(data.accessToken));
      const currentUser = await authApi.getCurrentUser();
      dispatch(setUser(currentUser));
      dispatch(finishHydration());
    } catch (err: unknown) {
      setError(getToastMessage(parseApiError(err)));
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="login-container">
      <div className="login-card">
        <div className="login-header">
          <h1>Prism ERP</h1>
          <p>Enterprise Management System</p>
        </div>

        <form onSubmit={handleSubmit} className="login-form">
          <div className="form-group">
            <label htmlFor="email">Email</label>
            <input
              type="email"
              id="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              placeholder="email@example.com"
              required
            />
          </div>

          <div className="form-group">
            <label htmlFor="password">Password</label>
            <input
              type="password"
              id="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              placeholder="••••••••"
              required
            />
          </div>

          {error && <div className="error-message">{error}</div>}

          <button type="submit" className="login-button" disabled={isLoading}>
            {isLoading ? 'Logging in...' : 'Login'}
          </button>
        </form>

        <div className="login-footer">
          <p>© 2026 Prism ERP. All rights reserved.</p>
        </div>
      </div>
    </div>
  );
}
