import { Link } from "react-router-dom";

function TermsOfServicePage() {
  return (
    <div className="legal-page">
      <Link to="/" className="legal-back-link">
        ← Back to Vault
      </Link>

      <h1>Terms of Service</h1>
      <p className="legal-updated">Last updated: [date of first publish]</p>

      <h2>1. What this is</h2>
      <p>
        Video Link Vault is an independent, non-commercial project built by
        Timothy Eckart. By creating an account, you agree to these terms.
      </p>

      <h2>2. Your account</h2>
      <p>
        You're responsible for the account you create and for keeping your
        sign-in secure. Accounts are free; free accounts may save up to 30
        links. An administrator may grant an individual account unlimited saved
        links, at their sole discretion, with no guarantee of eligibility or
        timeline.
      </p>

      <h2>3. What you save here</h2>
      <p>
        Video Link Vault stores links, titles, categories, and tags that you
        provide — it does not host, re-host, or take ownership of any video
        content itself. You're responsible for only saving links you have the
        right to save, and content on the linked pages remains governed by the
        terms of whichever platform (TikTok, YouTube, Instagram, Facebook) it
        lives on.
      </p>

      <h2>4. Acceptable use</h2>
      <p>
        Don't use this app to store or organize links to content that is
        illegal, that infringes someone else's rights, or that violates the
        terms of service of the platform it's hosted on.
      </p>

      <h2>5. Account actions</h2>
      <p>
        An administrator may block an account that violates these terms, at
        their discretion. Blocking may be reversed at the administrator's
        discretion; it is not a guaranteed or automatic process.
      </p>

      <h2>6. No warranty</h2>
      <p>
        This app is provided "as is," as a student capstone project, with no
        guarantee of uptime, data durability, or continued availability. Back up
        anything you can't afford to lose.
      </p>

      <h2>7. Limitation of liability</h2>
      <p>
        To the fullest extent permitted by law, Timothy Eckart is not liable for
        any damages arising from your use of this app, including lost data.
      </p>

      <h2>8. Changes</h2>
      <p>
        These terms may change as the project evolves; continued use after a
        change means you accept the update.
      </p>

      <h2>9. Contact</h2>
      <p>
        <a href="mailto:[contact email]">[contact email]</a>
      </p>
    </div>
  );
}

export default TermsOfServicePage;
