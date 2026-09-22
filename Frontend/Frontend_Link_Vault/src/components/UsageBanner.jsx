function UsageBanner({ usedCount, limit }) {
  const percentUsed = Math.min((usedCount / limit) * 100, 100);

  return (
    <div className="usage-banner">
      <span className="usage-text">
        {usedCount} / {limit} links used
      </span>
      <div className="usage-bar">
        <div className="usage-bar-fill" style={{ width: `${percentUsed}%` }} />
      </div>
    </div>
  );
}

export default UsageBanner;
