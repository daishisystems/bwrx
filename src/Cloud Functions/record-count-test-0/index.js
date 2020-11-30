/**
 * Responds to any HTTP request.
 *
 * @param {!express:Request} req HTTP request context.
 * @param {!express:Response} res HTTP response context.
 */
exports.getRecordCount = (req, res) => {
  res.status(200).send([{total: 8}]);
};
