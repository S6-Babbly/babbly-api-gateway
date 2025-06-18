import http from 'k6/http';
import { check, sleep } from 'k6';
import { Rate, Counter, Trend } from 'k6/metrics';

// Custom metrics
export let errorRate = new Rate('errors');
export let responseTime = new Trend('custom_response_time');
export let concurrentUsers = new Trend('concurrent_users');

// Test configuration for stress test
export let options = {
  stages: [
    { duration: '5m', target: 50 },   // Ramp up to 50 users (2.5x normal)
    { duration: '10m', target: 50 },  // Hold at 50 users
    { duration: '3m', target: 100 },  // Spike to 100 users (5x normal)
    { duration: '10m', target: 100 }, // Hold at peak stress
    { duration: '5m', target: 50 },   // Reduce back to 50
    { duration: '5m', target: 0 },    // Ramp down
  ],
  thresholds: {
    http_req_duration: ['p(95)<2000', 'p(99)<5000'], // More lenient during stress
    http_req_failed: ['rate<0.15'],                   // Allow higher error rate (15%)
    errors: ['rate<0.15'],                            // Custom error rate under 15%
  },
};

// Base URL for your deployment
const BASE_URL = 'http://172.212.0.150:5010';

// Test bearer token - REPLACE WITH ACTUAL TOKEN FROM AUTH0 OR USE THE MOCK TOKEN BELOW
// Mock JWT token from test-auth-service.http (for testing purposes only)
const TEST_TOKEN = '2UqYFxfnOQ2XHa.YsuCjvYX4e75Cw1ToDfRccrqE9ZI-1750265810494-0.0.1.1-604800000';

// Headers for authenticated requests
const authHeaders = {
  'Content-Type': 'application/json',
  'Authorization': `Bearer ${TEST_TOKEN}`
};

// Extended user pool for stress testing
const STRESS_USERS = [];
for (let i = 1; i <= 200; i++) {
  STRESS_USERS.push(`stress-user-${i.toString().padStart(3, '0')}`);
}

// High-frequency content for stress testing
const STRESS_CONTENT = [
  'Stress test message #${Math.random()}',
  'High load simulation post ${Date.now()}',
  'Testing system limits with concurrent users',
  'Performance testing in progress...',
  'Load testing API endpoints under stress',
  'Simulating peak traffic conditions',
  'Verifying system reliability under load',
  'Stress testing database performance'
];

function getRandomStressUser() {
  return STRESS_USERS[Math.floor(Math.random() * STRESS_USERS.length)];
}

function getStressContent() {
  const template = STRESS_CONTENT[Math.floor(Math.random() * STRESS_CONTENT.length)];
  return template.replace('${Math.random()}', Math.random().toString(36).substring(7))
                 .replace('${Date.now()}', Date.now().toString());
}

export default function () {
  const userId = getRandomStressUser();
  const startTime = Date.now();

  // Track concurrent users
  concurrentUsers.add(__VU);

  // Aggressive health checking (simulating monitoring tools)
  let response = http.get(`${BASE_URL}/health`);
  check(response, {
    'Health check under stress': (r) => r.status === 200,
  }) || errorRate.add(1);

  // Minimal sleep to increase pressure
  sleep(0.1);

  // High-frequency feed requests
  performFeedStressTest(userId);

  // Concurrent post creation (40% of users create posts)
  if (Math.random() < 0.4) {
    performPostStressTest(userId);
  }

  // Profile stress testing (60% of users check profiles)
  if (Math.random() < 0.6) {
    performProfileStressTest(userId);
  }

  // Services health check under load (20% of users)
  if (Math.random() < 0.2) {
    response = http.get(`${BASE_URL}/health/services`);
    check(response, {
      'Services health under stress': (r) => r.status === 200,
    }) || errorRate.add(1);
  }

  // Record response time
  responseTime.add(Date.now() - startTime);

  // Reduced sleep time to maintain pressure
  sleep(Math.random() * 0.5 + 0.1); // 0.1-0.6 seconds
}

function performFeedStressTest(userId) {
  // Multiple concurrent feed requests
  const feedRequests = [];
  const numRequests = Math.floor(Math.random() * 3) + 1; // 1-3 requests

  for (let i = 0; i < numRequests; i++) {
    const page = Math.floor(Math.random() * 5) + 1; // Pages 1-5
    const pageSize = Math.floor(Math.random() * 10) + 5; // 5-15 items
    
    let response = http.get(`${BASE_URL}/api/feed?page=${page}&pageSize=${pageSize}&userId=${userId}`);
    
    check(response, {
      'Feed stress test success': (r) => r.status === 200,
      'Feed stress response time OK': (r) => r.timings.duration < 3000,
    }) || errorRate.add(1);

    // Immediate post detail requests (aggressive behavior)
    if (response.status === 200 && Math.random() < 0.5) {
      const feedData = response.json();
      if (feedData.items && feedData.items.length > 0) {
        const randomPost = feedData.items[Math.floor(Math.random() * feedData.items.length)];
        
        response = http.get(`${BASE_URL}/api/feed/${randomPost.Id}?userId=${userId}`);
        check(response, {
          'Post details stress test': (r) => r.status === 200 || r.status === 404,
        }) || errorRate.add(1);
      }
    }

    sleep(0.05); // Very short pause between requests
  }
}

function performPostStressTest(userId) {
  // Burst post creation
  const numPosts = Math.floor(Math.random() * 3) + 1; // 1-3 posts rapidly

  for (let i = 0; i < numPosts; i++) {
    const postData = {
      Content: `${getStressContent()} - Burst ${i + 1} by ${userId}`,
      MediaUrl: Math.random() < 0.1 ? 'https://example.com/stress-image.jpg' : null
    };

    let response = http.post(`${BASE_URL}/api/posts`, JSON.stringify(postData), {
      headers: authHeaders
    });

    check(response, {
      'Post creation under stress': (r) => r.status === 200 || r.status === 201 || r.status === 401,
      'Post creation time reasonable': (r) => r.timings.duration < 2000,
    }) || errorRate.add(1);

    sleep(0.1); // Minimal delay between posts
  }
}

function performProfileStressTest(userId) {
  // Rapid profile checking
  const profiles = [userId, getRandomStressUser(), getRandomStressUser()];
  
  for (let profileId of profiles) {
    let response = http.get(`${BASE_URL}/api/profiles/id/${profileId}?postsPage=1&postsPageSize=20`);
    
    check(response, {
      'Profile stress test': (r) => r.status === 200 || r.status === 404,
      'Profile response time under stress': (r) => r.timings.duration < 2500,
    }) || errorRate.add(1);

    // Test "my profile" endpoint aggressively
    if (Math.random() < 0.3) {
      response = http.get(`${BASE_URL}/api/profiles/me?userId=${profileId}&postsPage=1&postsPageSize=10`);
      check(response, {
        'My profile stress test': (r) => r.status === 200 || r.status === 404,
      }) || errorRate.add(1);
    }

    sleep(0.05);
  }
}

export function setup() {
  console.log('Starting stress test...');
  console.log('This test will push the system beyond normal operating conditions');
  console.log('Expected behavior: Higher response times and some errors are acceptable');
  
  // Warm-up the system
  console.log('Warming up the system...');
  http.get(`${BASE_URL}/health`);
  
  return { startTime: Date.now() };
}

export function teardown(data) {
  const duration = (Date.now() - data.startTime) / 1000 / 60; // minutes
  console.log(`Stress test completed after ${duration.toFixed(2)} minutes`);
  console.log('System behavior under stress has been evaluated');
  console.log('Check metrics for performance degradation patterns');
} 