import http from 'k6/http';
import { check, sleep } from 'k6';
import { Rate, Counter, Trend } from 'k6/metrics';

// Custom metrics
export let errorRate = new Rate('errors');
export let postsCreated = new Counter('posts_created');
export let feedReads = new Counter('feed_reads');

// Test configuration for average load
export let options = {
  stages: [
    { duration: '2m', target: 10 },  // Ramp up to 10 users
    { duration: '5m', target: 10 },  // Stay at 10 users
    { duration: '2m', target: 20 },  // Ramp up to 20 users  
    { duration: '5m', target: 20 },  // Stay at 20 users
    { duration: '2m', target: 10 },  // Ramp down to 10 users
    { duration: '3m', target: 0 },   // Ramp down to 0 users
  ],
  thresholds: {
    http_req_duration: ['p(95)<1000', 'p(99)<2000'], // 95% under 1s, 99% under 2s
    http_req_failed: ['rate<0.05'],                   // Error rate under 5%
    errors: ['rate<0.05'],                            // Custom error rate under 5%
  },
};

// Base URL for AKS deployment
const BASE_URL = 'http://YOUR_AKS_LOADBALANCER_IP:5010';

// Weighted user scenarios
const USER_SCENARIOS = [
  { weight: 50, type: 'browser' },    // 50% browsing users
  { weight: 30, type: 'active' },     // 30% active users (post + browse)
  { weight: 20, type: 'heavy' },      // 20% heavy users (lots of activity)
];

// Sample user data
const USERS = [
  'user-001', 'user-002', 'user-003', 'user-004', 'user-005',
  'user-006', 'user-007', 'user-008', 'user-009', 'user-010',
  'demo-user-1', 'demo-user-2', 'demo-user-3'
];

// Sample content for posts
const POST_CONTENT = [
  'Just finished an amazing workout! 💪 #fitness #motivation',
  'Beautiful sunset today 🌅 Nature never fails to amaze me',
  'Working on some exciting new projects! Stay tuned... 🚀',
  'Coffee and code - the perfect combination ☕ #developer',
  'Weekend vibes! Time to relax and recharge 🌿',
  'Exploring new places and meeting new people 🌍 #travel',
  'Learning something new every day keeps life interesting 📚',
  'Grateful for all the amazing people in my life ❤️'
];

function getRandomUser() {
  return USERS[Math.floor(Math.random() * USERS.length)];
}

function getRandomPostContent() {
  return POST_CONTENT[Math.floor(Math.random() * POST_CONTENT.length)];
}

function selectUserScenario() {
  const random = Math.random() * 100;
  let cumulative = 0;
  
  for (let scenario of USER_SCENARIOS) {
    cumulative += scenario.weight;
    if (random <= cumulative) {
      return scenario.type;
    }
  }
  return 'browser'; // fallback
}

export default function () {
  const currentUser = getRandomUser();
  const scenario = selectUserScenario();
  
  // All users start with health check (simulating app load)
  let response = http.get(`${BASE_URL}/health`);
  check(response, {
    'Health check successful': (r) => r.status === 200,
  }) || errorRate.add(1);

  sleep(Math.random() * 2 + 1); // 1-3 seconds

  // All users browse the feed
  browseFeed(currentUser);

  if (scenario === 'active' || scenario === 'heavy') {
    // Active users also create posts and check profiles
    if (Math.random() < 0.7) { // 70% chance to create a post
      createPost(currentUser);
    }
    
    if (Math.random() < 0.5) { // 50% chance to check own profile
      checkProfile(currentUser);
    }
  }

  if (scenario === 'heavy') {
    // Heavy users do more browsing and interactions
    browseFeed(currentUser); // Browse again
    
    if (Math.random() < 0.8) { // 80% chance to check another profile
      checkProfile(getRandomUser());
    }
    
    if (Math.random() < 0.4) { // 40% chance to create another post
      createPost(currentUser);
    }
  }

  // Random pause between user sessions
  sleep(Math.random() * 3 + 2); // 2-5 seconds
}

function browseFeed(userId) {
  // Browse main feed
  let response = http.get(`${BASE_URL}/api/feed?page=1&pageSize=10&userId=${userId}`);
  check(response, {
    'Feed load successful': (r) => r.status === 200,
    'Feed has items': (r) => r.json('items') !== undefined,
  }) || errorRate.add(1);

  feedReads.add(1);

  if (response.status === 200) {
    const feedData = response.json();
    if (feedData.items && feedData.items.length > 0) {
      // Simulate reading post details (30% chance)
      if (Math.random() < 0.3) {
        const randomPost = feedData.items[Math.floor(Math.random() * feedData.items.length)];
        response = http.get(`${BASE_URL}/api/feed/${randomPost.Id}?userId=${userId}`);
        check(response, {
          'Post details load successful': (r) => r.status === 200 || r.status === 404,
        }) || errorRate.add(1);
      }
    }
  }

  sleep(Math.random() * 2 + 1); // 1-3 seconds reading time
}

function createPost(userId) {
  const postData = {
    Content: getRandomPostContent(),
    MediaUrl: Math.random() < 0.2 ? 'https://example.com/image.jpg' : null, // 20% with media
    UserId: userId
  };

  let response = http.post(`${BASE_URL}/api/posts`, JSON.stringify(postData), {
    headers: {
      'Content-Type': 'application/json',
    },
  });

  check(response, {
    'Post creation successful': (r) => r.status === 200 || r.status === 201,
  }) || errorRate.add(1);

  if (response.status === 200 || response.status === 201) {
    postsCreated.add(1);
  }

  sleep(Math.random() * 1 + 0.5); // 0.5-1.5 seconds
}

function checkProfile(userId) {
  // Check profile by ID
  let response = http.get(`${BASE_URL}/api/profiles/id/${userId}?postsPage=1&postsPageSize=5`);
  check(response, {
    'Profile load successful': (r) => r.status === 200 || r.status === 404,
  }) || errorRate.add(1);

  sleep(Math.random() * 2 + 1); // 1-3 seconds viewing time
}

export function setup() {
  console.log('Starting average load test...');
  console.log('This test simulates normal user behavior patterns');
  return {};
}

export function teardown(data) {
  console.log('Average load test completed.');
  console.log(`Total posts created: ${postsCreated.value || 0}`);
  console.log(`Total feed reads: ${feedReads.value || 0}`);
} 