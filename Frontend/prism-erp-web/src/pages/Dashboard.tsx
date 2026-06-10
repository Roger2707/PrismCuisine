import './Dashboard.css';

interface DashboardProps {
  username: string;
}

export default function Dashboard({ username }: DashboardProps) {
  const stats = [
    { title: 'Total Orders', value: '1,234', change: '+12%', icon: '📋', color: '#667eea' },
    { title: 'Revenue', value: '₫ 45.2M', change: '+8%', icon: '💰', color: '#10b981' },
    { title: 'Customers', value: '89', change: '+5%', icon: '👥', color: '#f59e0b' },
    { title: 'Products', value: '156', change: '+3%', icon: '📦', color: '#ef4444' },
  ];

  const recentActivities = [
    { id: 1, action: 'New Order', description: 'SO-2024-001 - Sen Vang Restaurant', time: '5 mins ago' },
    { id: 2, action: 'Order Confirmed', description: 'SO-2024-002 - Com Nieu Restaurant', time: '15 mins ago' },
    { id: 3, action: 'Goods Received', description: 'PO-2024-003 - Da Lat Fresh Vegetables', time: '1 hour ago' },
    { id: 4, action: 'New Customer', description: 'Riverside Hotel', time: '2 hours ago' },
  ];

  return (
    <div className="dashboard">
      <div className="dashboard-header">
        <h1>Hello, {username}!</h1>
        <p>Welcome back to Prism ERP</p>
      </div>

      <div className="stats-grid">
        {stats.map((stat) => (
          <div key={stat.title} className="stat-card">
            <div className="stat-icon" style={{ backgroundColor: `${stat.color}20` }}>
              <span>{stat.icon}</span>
            </div>
            <div className="stat-content">
              <h3>{stat.title}</h3>
              <p className="stat-value">{stat.value}</p>
              <span className="stat-change" style={{ color: stat.color }}>
                {stat.change}
              </span>
            </div>
          </div>
        ))}
      </div>

      <div className="dashboard-content">
        <div className="recent-activities">
          <h2>Recent Activities</h2>
          <div className="activities-list">
            {recentActivities.map((activity) => (
              <div key={activity.id} className="activity-item">
                <div className="activity-info">
                  <h4>{activity.action}</h4>
                  <p>{activity.description}</p>
                </div>
                <span className="activity-time">{activity.time}</span>
              </div>
            ))}
          </div>
        </div>

        <div className="quick-actions">
          <h2>Quick Actions</h2>
          <div className="actions-grid">
            <button className="action-button">
              <span>➕</span>
              <span>Create Order</span>
            </button>
            <button className="action-button">
              <span>📦</span>
              <span>Check Inventory</span>
            </button>
            <button className="action-button">
              <span>👥</span>
              <span>Manage Customers</span>
            </button>
            <button className="action-button">
              <span>📊</span>
              <span>View Reports</span>
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}
