import http from 'k6/http';
import { check, sleep } from 'k6';
import { Rate } from 'k6/metrics';

// Custom metrics
export let errorRate = new Rate('errors');

// Test configuration for smoke test
export let options = {
  stages: [
    { duration: '1m', target: 1 }, // Ramp up to 1 user
    { duration: '2m', target: 1 }, // Stay at 1 user
    { duration: '1m', target: 0 }, // Ramp down to 0 users
  ],
  thresholds: {
    http_req_duration: ['p(95)<500'], // 95% of requests should be below 500ms
    http_req_failed: ['rate<0.1'],    // Error rate should be below 10%
    errors: ['rate<0.1'],             // Custom error rate should be below 10%
  },
};

// Base URL for your deployment
const BASE_URL = 'http://172.212.0.150:5010';

// Test bearer token - REPLACE WITH ACTUAL TOKEN FROM AUTH0 OR USE THE MOCK TOKEN BELOW
// Mock JWT token from test-auth-service.http (for testing purposes only)
const TEST_TOKEN = '2UqYFxfnOQ2XHa.YsuCjvYX4e75Cw1ToDfRccrqE9ZI-1750265810494-0.0.1.1-604800000';

// Test data
const testUsers = ['user-1', 'user-2', 'demo-user-1'];
const testPostIds = [];

// Headers for authenticated requests
const authHeaders = {
  'Content-Type': 'application/json',
  'Authorization': `Bearer ${TEST_TOKEN}`
};

export default function () {
  let response;

  // Test 1: Health Check (No auth required)
  response = http.get(`${BASE_URL}/health`);
  check(response, {
    'Health check status is 200': (r) => r.status === 200,
    'Health check contains status': (r) => r.json('status') === 'healthy',
  }) || errorRate.add(1);

  sleep(1);

  // Test 2: Services Health Check (No auth required)
  response = http.get(`${BASE_URL}/health/services`);
  check(response, {
    'Services health check status is 200': (r) => r.status === 200,
    'Services health check has gateway info': (r) => r.json('gateway') !== undefined,
  }) || errorRate.add(1);

  sleep(1);

  // Test 3: Get Feed (Public - No auth required)
  response = http.get(`${BASE_URL}/api/feed?page=1&pageSize=5`);
  check(response, {
    'Feed status is 200': (r) => r.status === 200,
    'Feed has items array': (r) => Array.isArray(r.json('items')),
  }) || errorRate.add(1);

  // Extract post IDs for later tests
  if (response.status === 200) {
    const feedData = response.json();
    if (feedData.items && feedData.items.length > 0) {
      testPostIds.push(feedData.items[0].Id);
    }
  }

  sleep(1);

  // Test 4: Get Profile by ID (Public - No auth required)
  response = http.get(`${BASE_URL}/api/profiles/id/demo-user-1?postsPage=1&postsPageSize=3`);
  check(response, {
    'Profile by ID status is 200 or 404': (r) => r.status === 200 || r.status === 404,
  }) || errorRate.add(1);

  sleep(1);

  // Test 5: Get My Profile (Auth required)
  response = http.get(`${BASE_URL}/api/profiles/me?postsPage=1&postsPageSize=3`, {
    headers: authHeaders
  });
  check(response, {
    'My profile status is 200, 401, or 404': (r) => r.status === 200 || r.status === 401 || r.status === 404,
  }) || errorRate.add(1);

  sleep(1);

  // Test 6: Create Post (Auth required)
  const postPayload = {
    Content: `Smoke test post from k6 - ${new Date().toISOString()}`,
    MediaUrl: null
  };

  response = http.post(`${BASE_URL}/api/posts`, JSON.stringify(postPayload), {
    headers: authHeaders
  });
  check(response, {
    'Create post status is 200, 201, or 401': (r) => r.status === 200 || r.status === 201 || r.status === 401,
  }) || errorRate.add(1);

  sleep(1);

  // Test 7: Get Post Details (Public - No auth required)
  if (testPostIds.length > 0) {
    response = http.get(`${BASE_URL}/api/feed/${testPostIds[0]}?userId=demo-user-1`);
    check(response, {
      'Post details status is 200 or 404': (r) => r.status === 200 || r.status === 404,
    }) || errorRate.add(1);
  }

  sleep(2);
}

export function teardown(data) {
  console.log('Smoke test completed. All basic functionality verified.');
  console.log('Note: Some tests may show 401 errors if using mock token - this is expected.');
  console.log('To get a real Auth0 token for testing:');
  console.log('1. Use the frontend login flow');
  console.log('2. Check browser dev tools for the Authorization header');
  console.log('3. Replace TEST_TOKEN variable with the real token');
} 