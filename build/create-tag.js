module.exports = async ({tag, github, context}) => {
  // Check if tag already exists
  let existingTag = null;

  try {
    existingTag = await github.rest.git.getRef({
      owner: context.repo.owner,
      repo: context.repo.repo,
      ref: `tags/${tag}`
    });
  } catch (error) {
    if (error.status !== 404) {
      throw error;
    }

    // Tag doesn't exist, we can create it
    existingTag = null;
  }

  if (existingTag) {
    const existingSha = existingTag.data.object.sha;
    if (existingSha === context.sha) {
      console.log(`Tag ${tag} already exists with the same SHA (${context.sha}). Skipping tag creation.`);
      return;
    }

    throw new Error(`Tag ${tag} already exists with a different SHA. Existing: ${existingSha}, Current: ${context.sha}`);
  }

  // Create the tag
  await github.rest.git.createRef({
    owner: context.repo.owner,
    repo: context.repo.repo,
    ref: `refs/tags/${tag}`,
    sha: context.sha
  });
  console.log(`Tag ${tag} created successfully at ${context.sha}`);
};
