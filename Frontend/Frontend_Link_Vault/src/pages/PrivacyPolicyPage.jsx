import { Link } from "react-router-dom";

function PrivacyPolicyPage() {
  return (
    <div className="legal-page">
      <Link to="/" className="legal-back-link">
        ← Back to Vault
      </Link>

      <h1>Privacy Policy</h1>
      <p className="legal-updated">Last updated: [date of first publish]</p>

      <h2>What we collect</h2>
      <p>
        When you sign in, Microsoft Entra External ID (our identity provider)
        shares your email address and display name with us. When you use the
        app, we store the video links you save, along with any title, category,
        and tags you give them. We do not collect payment information, because
        we don't process payments.
      </p>

      <h2>How we use it</h2>
      <p>
        Your email and name identify your account and its saved links. Nothing
        you provide is sold, and nothing is used for advertising — this app
        doesn't run ads or third-party trackers.
      </p>

      <h2>Third parties involved</h2>
      <p>
        Authentication runs through Microsoft Entra External ID. The app itself
        and its data run on Microsoft Azure, in [Azure region]. Thumbnail images
        for YouTube and TikTok links are requested directly from those
        platforms' own public endpoints when you save a link — no personal data
        is sent to them beyond the link's own URL.
      </p>

      <h2>How long we keep it</h2>
      <p>
        Your data is kept as long as your account exists. You can request
        account and data deletion at any time by contacting{" "}
        <a href="mailto:[contact email]">[contact email]</a>; it will be deleted
        within [X days].
      </p>

      <h2>Children's privacy</h2>
      <p>
        This app is not directed at, and is not knowingly used to collect data
        from, children under 13.
      </p>

      <h2>Changes to this policy</h2>
      <p>
        If this policy changes, the "last updated" date above will change with
        it. Continued use of the app after a change means you accept the update.
      </p>

      <h2>Contact</h2>
      <p>
        <a href="mailto:[contact email]">[contact email]</a>
      </p>
    </div>
  );
}

export default PrivacyPolicyPage;
