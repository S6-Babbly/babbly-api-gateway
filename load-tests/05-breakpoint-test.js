import http from 'k6/http';
import { check, sleep } from 'k6';
import { Rate, Counter, Trend, Gauge } from 'k6/metrics';

// Custom metrics for breakpoint analysis
export let errorRate = new Rate('errors');
export let criticalErrors = new Counter('critical_errors');
export let responseTime = new Trend('response_time_trend');
export let throughput = new Counter('successful_requests');
export let activeUsers = new Gauge('active_users');

// Test configuration for breakpoint test - gradual increase until failure
export let options = {
  stages: [
    { duration: '5m', target: 25 },   // Start conservative
    { duration: '5m', target: 50 },   // Double load
    { duration: '5m', target: 100 },  // Double again
    { duration: '5m', target: 200 },  // Aggressive increase
    { duration: '5m', target: 400 },  // Very high load
    { duration: '5m', target: 600 },  // Extreme load
    { duration: '5m', target: 800 },  // Push to breaking point
    { duration: '5m', target: 1000 }, // Maximum stress
    { duration: '3m', target: 1000 }, // Hold at maximum
    { duration: '5m', target: 0 },    // Gradual recovery
  ],
  thresholds: {
    // These will likely be exceeded - that's the point!
    http_req_duration: ['p(95)<5000'], // Very lenient
    http_req_failed: ['rate<0.50'],    // Allow 50% failure at breakpoint
    errors: ['rate<0.60'],             // Allow high error rate
  },
};

// Base URL for AKS deployment
const BASE_URL = 'http://YOUR_AKS_LOADBALANCER_IP:5010';

// Large user pool for breakpoint testing
const BREAKPOINT_USERS = [];
for (let i = 1; i <= 1500; i++) {
  BREAKPOINT_USERS.push(`bp-user-${i.toString().padStart(4, '0')}`);
}

// Different load patterns to test various failure modes
const LOAD_PATTERNS = [
  'database_intensive',  // Heavy on data operations
  'cpu_intensive',      // Computationally heavy
  'memory_intensive',   // Large payloads
  'network_intensive'   // Many rapid requests
];

// Breakpoint-specific content with varying sizes
const BREAKPOINT_CONTENT = {
  small: 'Breakpoint test - small payload',
  medium: 'Breakpoint test with medium payload content '.repeat(10),
  large: 'Breakpoint test with large payload content that will consume more memory and bandwidth '.repeat(50),
  huge: 'Breakpoint test with huge payload designed to stress system limits with very long content '.repeat(200)
};

let currentLoad = 0;
let breakpointReached = false;
let consecutiveFailures = 0;
let maxSuccessfulLoad = 0;

function getRandomBreakpointUser() {
  return BREAKPOINT_USERS[Math.floor(Math.random() * BREAKPOINT_USERS.length)];
}

function getLoadPattern() {
  return LOAD_PATTERNS[Math.floor(Math.random() * LOAD_PATTERNS.length)];
}

function getVariableContent() {
  const sizes = Object.keys(BREAKPOINT_CONTENT);
  const randomSize = sizes[Math.floor(Math.random() * sizes.length)];
  return BREAKPOINT_CONTENT[randomSize] + ` - VU:${__VU} Time:${Date.now()}`;
}

function analyzeSystemHealth(response) {
  if (response.status >= 500) {
    criticalErrors.add(1);
    consecutiveFailures++;
  } else if (response.status >= 200 && response.status < 300) {
    throughput.add(1);
    consecutiveFailures = 0;
    if (__VU > maxSuccessfulLoad) {
      maxSuccessfulLoad = __VU;
    }
  }

  // Detect breakpoint
  if (consecutiveFailures > 10 && !breakpointReached) {
    breakpointReached = true;
    console.log(`BREAKPOINT REACHED at ${__VU} virtual users`);
    console.log(`Response time: ${response.timings.duration}ms`);
    console.log(`Status: ${response.status}`);
  }

  responseTime.add(response.timings.duration);
  activeUsers.add(__VU);
}

export default function () {
  const userId = getRandomBreakpointUser();
  const pattern = getLoadPattern();
  
  // Execute different load patterns to find various failure modes
  switch (pattern) {
    case 'database_intensive':
      performDatabaseIntensiveLoad(userId);
      break;
    case 'cpu_intensive':
      performCpuIntensiveLoad(userId);
      break;
    case 'memory_intensive':
      performMemoryIntensiveLoad(userId);
      break;
    case 'network_intensive':
      performNetworkIntensiveLoad(userId);
      break;
  }

  // Adaptive sleep based on system response
  const sleepTime = breakpointReached ? 
    Math.random() * 0.1 + 0.05 : // Very aggressive when pushing limits
    Math.random() * 0.3 + 0.1;   // Still aggressive but sustainable

  sleep(sleepTime);
}

function performDatabaseIntensiveLoad(userId) {
  // Heavy database operations - multiple feed requests with large page sizes
  
  let response = http.get(`${BASE_URL}/health/services`);
  check(response, {
    'DB intensive - services health': (r) => r.status === 200,
  }) || errorRate.add(1);
  analyzeSystemHealth(response);

  // Large feed requests
  for (let page = 1; page <= 5; page++) {
    response = http.get(`${BASE_URL}/api/feed?page=${page}&pageSize=50&userId=${userId}`);
    check(response, {
      'DB intensive - large feed request': (r) => r.status === 200,
      'DB intensive - reasonable response time': (r) => r.timings.duration < 10000,
    }) || errorRate.add(1);
    analyzeSystemHealth(response);

    // Profile requests with large post counts
    if (page <= 2) {
      response = http.get(`${BASE_URL}/api/profiles/id/${userId}?postsPage=${page}&postsPageSize=30`);
      check(response, {
        'DB intensive - profile with posts': (r) => r.status === 200 || r.status === 404,
      }) || errorRate.add(1);
      analyzeSystemHealth(response);
    }

    sleep(0.02);
  }
}

function performCpuIntensiveLoad(userId) {
  // CPU intensive operations - rapid requests
  
  for (let i = 0; i < 8; i++) {
    let response = http.get(`${BASE_URL}/health`);
    check(response, {
      'CPU intensive - rapid health checks': (r) => r.status === 200,
      'CPU intensive - fast response': (r) => r.timings.duration < 5000,
    }) || errorRate.add(1);
    analyzeSystemHealth(response);

    sleep(0.01); // Very rapid requests
  }

  // Rapid feed browsing
  for (let i = 0; i < 5; i++) {
    let response = http.get(`${BASE_URL}/api/feed?page=1&pageSize=20&userId=${userId}`);
    check(response, {
      'CPU intensive - rapid feed access': (r) => r.status === 200,
    }) || errorRate.add(1);
    analyzeSystemHealth(response);
  }
}

function performMemoryIntensiveLoad(userId) {
  // Memory intensive operations - large payloads
  
  // Create posts with large content
  for (let i = 0; i < 3; i++) {
    const largePostData = {
      Content: getVariableContent(),
      MediaUrl: 'https://example.com/large-image-' + Math.random() + '.jpg',
      UserId: userId
    };

    let response = http.post(`${BASE_URL}/api/posts`, JSON.stringify(largePostData), {
      headers: {
        'Content-Type': 'application/json',
      },
    });

    check(response, {
      'Memory intensive - large post creation': (r) => r.status === 200 || r.status === 201,
      'Memory intensive - post response time': (r) => r.timings.duration < 8000,
    }) || errorRate.add(1);
    analyzeSystemHealth(response);

    sleep(0.05);
  }

  // Request large feeds
  let response = http.get(`${BASE_URL}/api/feed?page=1&pageSize=100&userId=${userId}`);
  check(response, {
    'Memory intensive - large feed': (r) => r.status === 200,
  }) || errorRate.add(1);
  analyzeSystemHealth(response);
}

function performNetworkIntensiveLoad(userId) {
  // Network intensive - many small, rapid requests
  
  const requests = [];
  
  // Burst of concurrent-like requests
  for (let i = 0; i < 15; i++) {
    let response = http.get(`${BASE_URL}/health`);
    check(response, {
      'Network intensive - burst health check': (r) => r.status === 200,
    }) || errorRate.add(1);
    analyzeSystemHealth(response);

    // Mix in other endpoints
    if (i % 3 === 0) {
      response = http.get(`${BASE_URL}/api/feed?page=1&pageSize=5&userId=${userId}`);
      check(response, {
        'Network intensive - mini feed': (r) => r.status === 200,
      }) || errorRate.add(1);
      analyzeSystemHealth(response);
    }

    if (i % 5 === 0) {
      response = http.get(`${BASE_URL}/api/profiles/me?userId=${userId}`);
      check(response, {
        'Network intensive - profile check': (r) => r.status === 200 || r.status === 404,
      }) || errorRate.add(1);
      analyzeSystemHealth(response);
    }

    sleep(0.01); // Very rapid fire
  }
}

export function setup() {
  console.log('Starting breakpoint test...');
  console.log('This test will gradually increase load until system failure');
  console.log('Goal: Find maximum sustainable load and failure modes');
  console.log('Expected: System will eventually fail - this is intentional');
  
  // Initial system verification
  const response = http.get(`${BASE_URL}/health`);
  if (response.status !== 200) {
    console.error('System not healthy at start - aborting breakpoint test');
    throw new Error('System unhealthy');
  }

  return { 
    testStart: Date.now(),
    initialHealth: response.status
  };
}

export function teardown(data) {
  const duration = (Date.now() - data.testStart) / 1000 / 60;
  
  console.log('=== BREAKPOINT TEST RESULTS ===');
  console.log(`Test duration: ${duration.toFixed(2)} minutes`);
  console.log(`Maximum successful load: ${maxSuccessfulLoad} virtual users`);
  console.log(`Breakpoint reached: ${breakpointReached ? 'YES' : 'NO'}`);
  console.log(`Total critical errors: ${criticalErrors.value || 0}`);
  console.log(`Total successful requests: ${throughput.value || 0}`);
  
  console.log('\n=== CAPACITY ANALYSIS ===');
  if (maxSuccessfulLoad > 0) {
    console.log(`Recommended maximum load: ${Math.floor(maxSuccessfulLoad * 0.7)} users (70% of max)`);
    console.log(`Safe operating capacity: ${Math.floor(maxSuccessfulLoad * 0.5)} users (50% of max)`);
  }
  
  console.log('\n=== NEXT STEPS ===');
  console.log('1. Review error patterns and failure modes');
  console.log('2. Identify bottlenecks (CPU, memory, database, network)');
  console.log('3. Plan infrastructure scaling strategies');
  console.log('4. Set up monitoring alerts based on these thresholds');
} 