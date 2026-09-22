import { Link } from "react-router-dom";

const FAQS = [
  {
    q: "Is Video Link Vault free?",
    a: "Yes. Every account gets up to 30 saved links at no cost. There is no paid tier and no payment information is ever collected.",
  },
  {
    q: "How do I get more than 30 links?",
    a: "Unlimited access is granted individually by an admin — there's no self-serve upgrade or purchase. If you need more room, [contact email].",
  },
  {
    q: "Why don't Instagram and Facebook links show a real thumbnail?",
    a: "As of November 2025, Meta (Instagram/Facebook's parent company) stopped including thumbnail images in the public data its platforms share with apps like this one. YouTube and TikTok links still get real thumbnails; Instagram and Facebook links show a placeholder image instead. This isn't something we can fix on our end.",
  },
  {
    q: "Is my payment information stored anywhere?",
    a: "No — Video Link Vault doesn't process payments at all, so there's nothing to store.",
  },
  {
    q: "Can I delete my account and my data?",
    a: "Yes — [contact email] to request deletion. (A self-service delete-my-account option is planned before any App Store/Play Store submission.)",
  },
  {
    q: "Is there a mobile app?",
    a: "A companion mobile app is planned but not guaranteed for this project's current deadline. If it ships, it will be free with no in-app purchases.",
  },
];

function FaqPage() {
  return (
    <div className="legal-page">
      <Link to="/" className="legal-back-link">
        ← Back to Vault
      </Link>

      <h1>Frequently Asked Questions</h1>

      {FAQS.map((item) => (
        <div key={item.q} className="faq-item">
          <h2>{item.q}</h2>
          <p>{item.a}</p>
        </div>
      ))}
    </div>
  );
}

export default FaqPage;
