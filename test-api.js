const axios = require('axios');

// 测试不同的端口
const ports = [5123, 7289, 5000, 5001, 44300];

async function testAPI() {
  console.log('Testing API connection...');
  
  for (const port of ports) {
    try {
      console.log(`Testing port ${port}...`);
      const response = await axios.get(`http://localhost:${port}/api/questions`);
      console.log(`Success! API is running on port ${port}`);
      console.log(`Response status: ${response.status}`);
      return port; // 返回成功的端口
    } catch (error) {
      if (error.code === 'ECONNREFUSED') {
        console.log(`Port ${port} is not available`);
      } else {
        console.log(`Error on port ${port}: ${error.message}`);
      }
    }
  }
  
  console.log('Could not connect to API on any port');
  return null;
}

testAPI().then(port => {
  if (port) {
    console.log(`\nUse port ${port} in your importQuestions.cjs file`);
  } else {
    console.log('\nPlease make sure the API server is running');
  }
});
