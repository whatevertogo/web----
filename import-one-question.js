const axios = require('axios');

// 测试不同的端口
const ports = [5123, 7289, 5000, 5001, 44300];

// 一个简单的题目
const question = {
  type: 0,
  content: '测试题目',
  optionsJson: JSON.stringify(['选项A', '选项B', '选项C', '选项D']),
  answersJson: JSON.stringify(['选项A']),
  analysis: '这是一个测试题目',
  difficulty: 1,
  createTime: new Date().toISOString()
};

async function importQuestion() {
  console.log('Importing a test question...');
  
  for (const port of ports) {
    try {
      console.log(`Testing port ${port}...`);
      const response = await axios.post(`http://localhost:${port}/api/questions`, question);
      console.log(`Success! Question imported on port ${port}`);
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
  
  console.log('Could not import question on any port');
  return null;
}

importQuestion().then(port => {
  if (port) {
    console.log(`\nUse port ${port} in your importQuestions.cjs file`);
  } else {
    console.log('\nPlease make sure the API server is running');
  }
});
