import http from 'k6/http';
import { check, sleep } from 'k6';
import { Rate, Counter, Trend } from 'k6/metrics';

// Custom metrics
export let errorRate = new Rate('errors');
export let spikeRecoveryTime = new Trend('spike_recovery_time');
export let requestsPerSecond = new Counter('requests_per_second');

// Test configuration for spike test
export let options = {
  stages: [
    { duration: '2m', target: 10 },   // Normal load baseline
    { duration: '1m', target: 10 },   // Hold baseline
    { duration: '10s', target: 200 }, // SPIKE! 20x increase in 10 seconds
    { duration: '3m', target: 200 },  // Hold spike load
    { duration: '10s', target: 10 },  // Drop back to normal
    { duration: '3m', target: 10 },   // Recovery period
    { duration: '10s', target: 150 }, // Second smaller spike
    { duration: '2m', target: 150 },  // Hold second spike
    { duration: '2m', target: 0 },    // Ramp down
  ],
  thresholds: {
    http_req_duration: ['p(95)<3000'], // Allow higher response times during spikes
    http_req_failed: ['rate<0.20'],    // Allow 20% errors during extreme spikes
    errors: ['rate<0.20'],             // Custom error rate threshold
  },
};

// Base URL for AKS deployment
const BASE_URL = 'http://YOUR_AKS_LOADBALANCER_IP:5010';

// Spike test users
const SPIKE_USERS = [];
for (let i = 1; i <= 300; i++) {
  SPIKE_USERS.push(`spike-user-${i.toString().padStart(3, '0')}`);
}

// Spike-specific content
const SPIKE_CONTENT = [
  'BREAKING: Major event happening now! 🚨',
  'VIRAL: This is trending everywhere! 🔥',
  'LIVE UPDATE: Breaking news alert #${timestamp}',
  'URGENT: Everyone needs to see this now!',
  'SPIKE TEST: Simulating viral content load',
  'TRENDING: Massive traffic incoming #viral',
  'HOT TOPIC: Sudden surge in user activity',
  'ALERT: Peak traffic simulation in progress'
];

let currentStage = 'baseline';
let spikeStartTime = null;

function getRandomSpikeUser() {
  return SPIKE_USERS[Math.floor(Math.random() * SPIKE_USERS.length)];
}

function getSpikeContent() {
  const template = SPIKE_CONTENT[Math.floor(Math.random() * SPIKE_CONTENT.length)];
  return template.replace('#${timestamp}', Date.now());
}

function detectSpike() {
  // Detect when we're in a spike based on VU count
  if (__VU > 150 && currentStage !== 'spike') {
    currentStage = 'spike';
    spikeStartTime = Date.now();
    console.log(`SPIKE DETECTED at VU: ${__VU}`);
  } else if (__VU <= 50 && currentStage === 'spike') {
    currentStage = 'recovery';
    if (spikeStartTime) {
      spikeRecoveryTime.add(Date.now() - spikeStartTime);
    }
    console.log(`SPIKE RECOVERY at VU: ${__VU}`);
  }
}

export default function () {
  const userId = getRandomSpikeUser();
  detectSpike();
  
  const startTime = Date.now();
  requestsPerSecond.add(1);

  // Behavior changes based on spike phase
  if (currentStage === 'spike') {
    performSpikeLoad(userId);
  } else if (currentStage === 'recovery') {
    performRecoveryLoad(userId);
  } else {
    performBaselineLoad(userId);
  }

  // Adaptive sleep based on current load
  const sleepTime = currentStage === 'spike' ? 
    Math.random() * 0.2 + 0.1 : // Very fast during spike (0.1-0.3s)
    Math.random() * 1 + 0.5;     // Normal during baseline (0.5-1.5s)
  
  sleep(sleepTime);
}

function performBaselineLoad(userId) {
  // Normal user behavior - health check and feed browsing
  let response = http.get(`${BASE_URL}/health`);
  check(response, {
    'Baseline health check': (r) => r.status === 200,
  }) || errorRate.add(1);

  // Normal feed browsing
  response = http.get(`${BASE_URL}/api/feed?page=1&pageSize=10&userId=${userId}`);
  check(response, {
    'Baseline feed access': (r) => r.status === 200,
    'Baseline response time OK': (r) => r.timings.duration < 1000,
  }) || errorRate.add(1);

  // Occasional profile check
  if (Math.random() < 0.3) {
    response = http.get(`${BASE_URL}/api/profiles/me?userId=${userId}`);
    check(response, {
      'Baseline profile check': (r) => r.status === 200 || r.status === 404,
    }) || errorRate.add(1);
  }
}

function performSpikeLoad(userId) {
  // Aggressive behavior during spike - simulating viral content scenario
  
  // Rapid health checks (monitoring/load balancer checks)
  let response = http.get(`${BASE_URL}/health`);
  check(response, {
    'Spike health check': (r) => r.status === 200,
    'Spike health response fast': (r) => r.timings.duration < 2000,
  }) || errorRate.add(1);

  // Burst feed requests (users refreshing to see viral content)
  for (let i = 0; i < 3; i++) {
    response = http.get(`${BASE_URL}/api/feed?page=${i+1}&pageSize=20&userId=${userId}`);
    check(response, {
      'Spike feed request': (r) => r.status === 200,
      'Spike feed reasonable time': (r) => r.timings.duration < 5000,
    }) || errorRate.add(1);

    // Immediate post detail viewing
    if (response.status === 200) {
      const feedData = response.json();
      if (feedData.items && feedData.items.length > 0) {
        // View multiple posts rapidly
        for (let j = 0; j < Math.min(2, feedData.items.length); j++) {
          const post = feedData.items[j];
          response = http.get(`${BASE_URL}/api/feed/${post.Id}?userId=${userId}`);
          check(response, {
            'Spike post details': (r) => r.status === 200 || r.status === 404,
          }) || errorRate.add(1);
        }
      }
    }

    sleep(0.05); // Very short pause between requests
  }

  // Create viral content (50% of users during spike)
  if (Math.random() < 0.5) {
    const viralPost = {
      Content: `${getSpikeContent()} from ${userId}`,
      MediaUrl: Math.random() < 0.3 ? 'https://example.com/viral-image.jpg' : null,
      UserId: userId
    };

    response = http.post(`${BASE_URL}/api/posts`, JSON.stringify(viralPost), {
      headers: {
        'Content-Type': 'application/json',
      },
    });

    check(response, {
      'Spike post creation': (r) => r.status === 200 || r.status === 201,
      'Spike post creation time': (r) => r.timings.duration < 3000,
    }) || errorRate.add(1);
  }

  // Rapid profile checking (users looking at viral content creators)
  for (let i = 0; i < 2; i++) {
    const randomUser = getRandomSpikeUser();
    response = http.get(`${BASE_URL}/api/profiles/id/${randomUser}?postsPage=1&postsPageSize=5`);
    check(response, {
      'Spike profile check': (r) => r.status === 200 || r.status === 404,
    }) || errorRate.add(1);
    
    sleep(0.02);
  }
}

function performRecoveryLoad(userId) {
  // Recovery behavior - system stabilizing after spike
  
  // Health check to verify system stability
  let response = http.get(`${BASE_URL}/health/services`);
  check(response, {
    'Recovery services check': (r) => r.status === 200,
    'Recovery services response': (r) => r.timings.duration < 1500,
  }) || errorRate.add(1);

  // Moderate feed browsing
  response = http.get(`${BASE_URL}/api/feed?page=1&pageSize=15&userId=${userId}`);
  check(response, {
    'Recovery feed access': (r) => r.status === 200,
    'Recovery feed stable': (r) => r.timings.duration < 2000,
  }) || errorRate.add(1);

  // Profile checks to ensure data consistency
  response = http.get(`${BASE_URL}/api/profiles/me?userId=${userId}&postsPage=1&postsPageSize=10`);
  check(response, {
    'Recovery profile consistency': (r) => r.status === 200 || r.status === 404,
  }) || errorRate.add(1);

  sleep(0.1);
}

export function setup() {
  console.log('Starting spike test...');
  console.log('This test simulates sudden traffic surges (viral content, news events, etc.)');
  console.log('Testing auto-scaling, load balancing, and system resilience');
  
  // Verify baseline health
  const response = http.get(`${BASE_URL}/health`);
  if (response.status !== 200) {
    console.error('System not healthy before spike test!');
  }
  
  return { 
    testStart: Date.now(),
    baselineRps: 0
  };
}

export function teardown(data) {
  const duration = (Date.now() - data.testStart) / 1000 / 60;
  console.log(`Spike test completed after ${duration.toFixed(2)} minutes`);
  console.log('Spike test results:');
  console.log('- Check for auto-scaling behavior during spikes');
  console.log('- Verify system recovery after spike periods');
  console.log('- Monitor error rates during extreme load');
  console.log('- Assess response time degradation patterns');
} 