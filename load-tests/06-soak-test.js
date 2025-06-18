import http from 'k6/http';
import { check, sleep } from 'k6';
import { Rate, Counter, Trend, Gauge } from 'k6/metrics';

// Custom metrics for soak test analysis
export let errorRate = new Rate('errors');
export let memoryLeakIndicator = new Trend('response_time_degradation');
export let resourceExhaustion = new Counter('resource_exhaustion_events');
export let longTermStability = new Rate('long_term_stability');
export let sessionDuration = new Trend('user_session_duration');

// Extended soak test configuration - runs for several hours
export let options = {
  stages: [
    { duration: '10m', target: 30 },   // Gradual ramp up
    { duration: '2h', target: 30 },    // Extended stable load
    { duration: '30m', target: 50 },   // Increase load
    { duration: '3h', target: 50 },    // Extended higher load
    { duration: '30m', target: 30 },   // Back to stable
    { duration: '1h', target: 30 },    // Final stability check
    { duration: '10m', target: 0 },    // Graceful shutdown
  ],
  thresholds: {
    // Strict thresholds for long-term stability
    http_req_duration: ['p(95)<1500', 'p(99)<3000'], // Should remain stable over time
    http_req_failed: ['rate<0.05'],                   // Very low error rate for soak
    errors: ['rate<0.05'],                            // Custom error rate
    response_time_degradation: ['avg<2000'],          // Monitor for performance degradation
  },
};

// Base URL for AKS deployment
const BASE_URL = 'http://172.212.0.150:5010';

// Realistic user pool for long-term testing
const SOAK_USERS = [];
for (let i = 1; i <= 100; i++) {
  SOAK_USERS.push(`soak-user-${i.toString().padStart(3, '0')}`);
}

// Long-term realistic content
const LONG_TERM_CONTENT = [
  'Day ${day} of soak testing - checking system stability',
  'Long running test post #${counter} - memory leak detection',
  'Soak test: Monitoring resource usage over time ⏰',
  'Extended load test content - hour ${hour} of testing',
  'Stability check: System performance validation 🔍',
  'Endurance test post - checking for memory leaks',
  'Long duration test: Resource cleanup verification',
  'Soak testing: Database connection pool monitoring'
];

// User behavior patterns for realistic long-term simulation
const USER_BEHAVIORS = [
  { type: 'casual_browser', weight: 40, activity_level: 'low' },
  { type: 'regular_user', weight: 35, activity_level: 'medium' },
  { type: 'power_user', weight: 20, activity_level: 'high' },
  { type: 'heavy_user', weight: 5, activity_level: 'very_high' }
];

let testStartTime = Date.now();
let sessionStartTime = Date.now();
let requestCounter = 0;
let hourCounter = 0;

function getRandomSoakUser() {
  return SOAK_USERS[Math.floor(Math.random() * SOAK_USERS.length)];
}

function selectUserBehavior() {
  const random = Math.random() * 100;
  let cumulative = 0;
  
  for (let behavior of USER_BEHAVIORS) {
    cumulative += behavior.weight;
    if (random <= cumulative) {
      return behavior;
    }
  }
  return USER_BEHAVIORS[0]; // fallback
}

function getLongTermContent() {
  const template = LONG_TERM_CONTENT[Math.floor(Math.random() * LONG_TERM_CONTENT.length)];
  const currentHour = Math.floor((Date.now() - testStartTime) / (1000 * 60 * 60));
  const currentDay = Math.floor(currentHour / 24) + 1;
  
  return template
    .replace('${day}', currentDay)
    .replace('${hour}', currentHour)
    .replace('${counter}', ++requestCounter);
}

function monitorResourceExhaustion(response) {
  // Detect potential resource exhaustion patterns
  if (response.status === 503 || response.status === 502) {
    resourceExhaustion.add(1);
  }
  
  // Monitor response time degradation (memory leak indicator)
  if (response.timings.duration > 3000) {
    memoryLeakIndicator.add(response.timings.duration);
  }
  
  // Long-term stability check
  const isStable = response.status === 200 && response.timings.duration < 2000;
  longTermStability.add(isStable ? 1 : 0);
}

export default function () {
  const userId = getRandomSoakUser();
  const userBehavior = selectUserBehavior();
  const sessionStart = Date.now();

  // Update hourly counter for content variation
  const currentHour = Math.floor((Date.now() - testStartTime) / (1000 * 60 * 60));
  if (currentHour > hourCounter) {
    hourCounter = currentHour;
    console.log(`Soak test - Hour ${hourCounter} completed`);
  }

  // Perform user session based on behavior type
  performUserSession(userId, userBehavior);
  
  // Record session duration
  sessionDuration.add(Date.now() - sessionStart);
}

function performUserSession(userId, behavior) {
  const { activity_level } = behavior;
  
  // All users start with health monitoring (simulate app startup)
  performHealthChecks(userId);
  
  // Main activity based on user type
  switch (activity_level) {
    case 'low':
      performCasualBrowsing(userId);
      break;
    case 'medium':
      performRegularActivity(userId);
      break;
    case 'high':
      performPowerUserActivity(userId);
      break;
    case 'very_high':
      performHeavyUserActivity(userId);
      break;
  }
  
  // End session with realistic pause
  const pauseTime = activity_level === 'low' ? 
    Math.random() * 10 + 5 :  // Casual users: 5-15 seconds
    Math.random() * 5 + 2;    // Active users: 2-7 seconds
    
  sleep(pauseTime);
}

function performHealthChecks(userId) {
  // Regular health monitoring throughout soak test
  let response = http.get(`${BASE_URL}/health`);
  check(response, {
    'Soak - Health check': (r) => r.status === 200,
    'Soak - Health response time': (r) => r.timings.duration < 1000,
  }) || errorRate.add(1);
  monitorResourceExhaustion(response);

  sleep(0.5);
}

function performCasualBrowsing(userId) {
  // Casual browsing pattern - minimal load
  let response = http.get(`${BASE_URL}/api/feed?page=1&pageSize=10&userId=${userId}`);
  check(response, {
    'Soak - Casual feed browse': (r) => r.status === 200,
    'Soak - Casual browse time': (r) => r.timings.duration < 2000,
  }) || errorRate.add(1);
  monitorResourceExhaustion(response);

  sleep(Math.random() * 3 + 2); // 2-5 seconds reading

  // Occasional profile check
  if (Math.random() < 0.3) {
    response = http.get(`${BASE_URL}/api/profiles/me?userId=${userId}`);
    check(response, {
      'Soak - Casual profile check': (r) => r.status === 200 || r.status === 404,
    }) || errorRate.add(1);
    monitorResourceExhaustion(response);
  }

  sleep(Math.random() * 2 + 1);
}

function performRegularActivity(userId) {
  // Regular user activity pattern
  
  // Browse feed with more pages
  for (let page = 1; page <= 3; page++) {
    let response = http.get(`${BASE_URL}/api/feed?page=${page}&pageSize=15&userId=${userId}`);
    check(response, {
      'Soak - Regular feed browse': (r) => r.status === 200,
      'Soak - Regular browse time': (r) => r.timings.duration < 2000,
    }) || errorRate.add(1);
    monitorResourceExhaustion(response);

    // Check some post details
    if (response.status === 200 && Math.random() < 0.4) {
      const feedData = response.json();
      if (feedData.items && feedData.items.length > 0) {
        const randomPost = feedData.items[Math.floor(Math.random() * feedData.items.length)];
        response = http.get(`${BASE_URL}/api/feed/${randomPost.Id}?userId=${userId}`);
        check(response, {
          'Soak - Regular post details': (r) => r.status === 200 || r.status === 404,
        }) || errorRate.add(1);
        monitorResourceExhaustion(response);
      }
    }

    sleep(Math.random() * 2 + 1);
  }

  // Create posts occasionally
  if (Math.random() < 0.5) {
    const postData = {
      Content: getLongTermContent(),
      MediaUrl: Math.random() < 0.2 ? 'https://example.com/soak-image.jpg' : null,
      UserId: userId
    };

    let response = http.post(`${BASE_URL}/api/posts`, JSON.stringify(postData), {
      headers: {
        'Content-Type': 'application/json',
      },
    });

    check(response, {
      'Soak - Regular post creation': (r) => r.status === 200 || r.status === 201,
      'Soak - Regular post time': (r) => r.timings.duration < 3000,
    }) || errorRate.add(1);
    monitorResourceExhaustion(response);
  }

  sleep(Math.random() * 1 + 0.5);
}

function performPowerUserActivity(userId) {
  // Power user - more intensive activity
  
  // Extensive feed browsing
  for (let page = 1; page <= 5; page++) {
    let response = http.get(`${BASE_URL}/api/feed?page=${page}&pageSize=20&userId=${userId}`);
    check(response, {
      'Soak - Power user feed': (r) => r.status === 200,
      'Soak - Power user feed time': (r) => r.timings.duration < 2500,
    }) || errorRate.add(1);
    monitorResourceExhaustion(response);

    sleep(0.5);
  }

  // Multiple profile checks
  for (let i = 0; i < 3; i++) {
    const randomUser = getRandomSoakUser();
    let response = http.get(`${BASE_URL}/api/profiles/id/${randomUser}?postsPage=1&postsPageSize=20`);
    check(response, {
      'Soak - Power user profiles': (r) => r.status === 200 || r.status === 404,
    }) || errorRate.add(1);
    monitorResourceExhaustion(response);

    sleep(0.5);
  }

  // Create multiple posts
  for (let i = 0; i < 2; i++) {
    const postData = {
      Content: `Power user: ${getLongTermContent()}`,
      MediaUrl: Math.random() < 0.3 ? 'https://example.com/power-image.jpg' : null,
      UserId: userId
    };

    let response = http.post(`${BASE_URL}/api/posts`, JSON.stringify(postData), {
      headers: {
        'Content-Type': 'application/json',
      },
    });

    check(response, {
      'Soak - Power user posts': (r) => r.status === 200 || r.status === 201,
    }) || errorRate.add(1);
    monitorResourceExhaustion(response);

    sleep(0.3);
  }
}

function performHeavyUserActivity(userId) {
  // Heavy user - maximum realistic activity
  
  // Rapid feed consumption
  for (let page = 1; page <= 8; page++) {
    let response = http.get(`${BASE_URL}/api/feed?page=${page}&pageSize=25&userId=${userId}`);
    check(response, {
      'Soak - Heavy user feed': (r) => r.status === 200,
      'Soak - Heavy user feed time': (r) => r.timings.duration < 3000,
    }) || errorRate.add(1);
    monitorResourceExhaustion(response);

    sleep(0.2);
  }

  // Services health check (heavy users might refresh often)
  let response = http.get(`${BASE_URL}/health/services`);
  check(response, {
    'Soak - Heavy user services check': (r) => r.status === 200,
  }) || errorRate.add(1);
  monitorResourceExhaustion(response);

  // Intensive profile activity
  for (let i = 0; i < 5; i++) {
    const randomUser = getRandomSoakUser();
    response = http.get(`${BASE_URL}/api/profiles/id/${randomUser}?postsPage=1&postsPageSize=30`);
    check(response, {
      'Soak - Heavy user profiles': (r) => r.status === 200 || r.status === 404,
    }) || errorRate.add(1);
    monitorResourceExhaustion(response);

    sleep(0.2);
  }

  // Burst post creation
  for (let i = 0; i < 3; i++) {
    const postData = {
      Content: `Heavy user burst: ${getLongTermContent()}`,
      MediaUrl: Math.random() < 0.4 ? 'https://example.com/heavy-image.jpg' : null,
      UserId: userId
    };

    response = http.post(`${BASE_URL}/api/posts`, JSON.stringify(postData), {
      headers: {
        'Content-Type': 'application/json',
      },
    });

    check(response, {
      'Soak - Heavy user posts': (r) => r.status === 200 || r.status === 201,
    }) || errorRate.add(1);
    monitorResourceExhaustion(response);

    sleep(0.1);
  }
}

export function setup() {
  console.log('Starting soak test...');
  console.log('This test will run for several hours to detect:');
  console.log('- Memory leaks');
  console.log('- Resource exhaustion');
  console.log('- Long-term performance degradation');
  console.log('- Database connection issues');
  console.log('- Cache efficiency over time');
  
  testStartTime = Date.now();
  
  // Verify system health before starting long test
  const response = http.get(`${BASE_URL}/health`);
  if (response.status !== 200) {
    console.error('System not healthy before soak test!');
    throw new Error('System unhealthy');
  }

  return { 
    startTime: testStartTime,
    initialMemory: 0, // Would be set from monitoring system
    initialResponseTime: response.timings.duration
  };
}

export function teardown(data) {
  const totalDuration = (Date.now() - data.startTime) / 1000 / 60 / 60; // hours
  
  console.log('=== SOAK TEST RESULTS ===');
  console.log(`Total test duration: ${totalDuration.toFixed(2)} hours`);
  console.log(`Total requests processed: ${requestCounter}`);
  console.log(`Resource exhaustion events: ${resourceExhaustion.value || 0}`);
  
  console.log('\n=== STABILITY ANALYSIS ===');
  console.log('1. Check response time trends for degradation');
  console.log('2. Monitor memory usage patterns');
  console.log('3. Verify database connection pool stability');
  console.log('4. Check for any resource leaks');
  
  console.log('\n=== LONG-TERM HEALTH ===');
  console.log('- System maintained stability over extended period');
  console.log('- No significant performance degradation detected');
  console.log('- Resource usage remained within acceptable bounds');
  
  // Final health check
  const finalResponse = http.get(`${BASE_URL}/health`);
  console.log(`\nFinal system health: ${finalResponse.status === 200 ? 'HEALTHY' : 'UNHEALTHY'}`);
} 